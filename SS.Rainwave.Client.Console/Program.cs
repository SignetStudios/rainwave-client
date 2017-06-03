using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using log4net;
using SS.Rainwave.Client.Console.Properties;
using SS.Rainwave.Objects;
using SS.Rainwave.Objects.API;

namespace SS.Rainwave.Client.Console
{
	public class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
		private static Timer _autoPause;
		private static Timer _autoUnPause;

		#region Public Static Members

		public static void Main()
		{
			System.Console.SetWindowSize(75, 30);

			var work = new Workhorse();

			Log.Info("Starting RW Interface.");
			Log.Info("Hit enter to pause or unpause the request queue.");

			ThreadPool.QueueUserWorkItem(work.ExecuteWork);

			var curDateTime = DateTime.Now;
			var curTime = curDateTime.TimeOfDay;

			var pauseTime = Settings.Default.AutoPauseTime - curTime;
			var unpauseTime = Settings.Default.AutoUnpauseTime - curTime;

			pauseTime += pauseTime.TotalSeconds < 0 ? new TimeSpan(1, 0, 0, 0) : new TimeSpan(0); //Adjust to next runtime
			unpauseTime += unpauseTime.TotalSeconds < 0 ? new TimeSpan(1, 0, 0, 0) : new TimeSpan(0); //Adjust to next runtime

			_autoPause = new Timer(work.AutoPauseRequestQueue, null, pauseTime, new TimeSpan(1, 0, 0, 0));
			_autoUnPause = new Timer(work.AutoUnpauseRequestQueue, null, unpauseTime, new TimeSpan(1, 0, 0, 0));

			Log.Info($"Auto pause delayed until {curDateTime.Add(pauseTime)}.");
			Log.Info($"Auto unpause delayed until {curDateTime.Add(unpauseTime)}.");

			string con;
			var isPaused = work.IsPaused();

			Log.Info($"Requests currently {(isPaused ? "" : "un")}paused.");

			do
			{
				con = System.Console.ReadLine();

				if (!string.IsNullOrWhiteSpace(con))
				{
					continue;
				}

				isPaused = work.IsPaused();

				if (isPaused)
				{
					work.UnpauseRequestQueue();
				}
				else
				{
					work.PauseRequestQueue();
				}

				Log.Info($"Requests {(!isPaused ? "" : "un")}paused");

			} while (string.IsNullOrWhiteSpace(con));

			work.StopWork = true;
			work.AutoPauseRequestQueue(null);
			_autoPause.Dispose();
			_autoUnPause.Dispose();
		}

		#endregion
	}

	internal class Workhorse
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Workhorse));

		private readonly IRainwaveClient _client;
		public bool StopWork { get; set; }


		public Workhorse() : this(new Rainwave.RainwaveClient(Settings.Default.BaseApiUrl, Settings.Default.UserId, Settings.Default.ApiKey, (SiteId) Settings.Default.DefaultStation))
		{
		}

		internal Workhorse(IRainwaveClient rainwaveClient)
		{
			_client = rainwaveClient;
		}

		public void ClearRequestQueue()
		{
			_client.ClearRequestQueue();
		}

		public void AutoPauseRequestQueue(object timerObj)
		{
			if (IsPaused())
			{
				return;
			}

			PauseRequestQueue();
			Log.Info("Request queue auto paused.");
		}

		public void PauseRequestQueue()
		{
			_client.PauseRequestQueue(_client.CurrentSite);
		}

		public void AutoUnpauseRequestQueue(object timerObj)
		{
			if (!IsPaused())
			{
				return;
			}

			UnpauseRequestQueue();
			Log.Info("Request queue auto unpaused.");
		}

		public void UnpauseRequestQueue()
		{
			_client.UnpauseRequestQueue(_client.CurrentSite);
			//_client.FixRequestList(_client.GetInfo(_client.CurrentSite));
		}

		public bool IsPaused()
		{
			return _client.IsPaused();
		}

		public void ExecuteWork(object state)
		{
			var info = _client.GetInfo(_client.CurrentSite);

			updateRequestQueue(info);
			vote(_client.GetInfo(_client.CurrentSite));

			while (!StopWork)
			{
				var sync = _client.Sync(_client.CurrentSite);

				if (sync?.User == null)
				{
					continue;
				}

				if (!IsPaused() && !sync.User.TunedIn)
				{
					AutoPauseRequestQueue(null);
				}

				if (IsPaused()) continue;

				if (sync.SchedNext == null || !sync.SchedNext.Any())
				{
					continue;
				}

				updateRequestQueue(sync);

				if (sync.User.TunedIn)
				{
					vote(sync);
				}
			}
		}

		private void vote(Info info)
		{
			loadVotePriorities();

			_client.AutoVote(info);
		}

		private void loadVotePriorities()
		{
			if (_client.VotePriorities != null &&
			    File.GetLastWriteTime(Settings.Default.VotingPrefs) <= _client.VotePrioritiesLoaded)
			{
				return;
			}

			var fileContents = XDocument.Load(Settings.Default.VotingPrefs);

			if (fileContents.Root == null)
			{
				return;
			}

			var tempList = fileContents.Root.Elements()
				.Select(element => new
				                   {
					                   order = element?.Element("SortOrder")?.Value,
					                   request = element?.Element("IsRequest")?.Value,
					                   fav = element?.Element("IsFavorite")?.Value,
					                   mine = element?.Element("IsMyRequest")?.Value,
					                   rating = element?.Element("SongRating")?.Value
				                   })
				.Select(x => new VotePriority
				             {
					             SortOrder = int.Parse(x.order),
					             IsRequest = string.IsNullOrEmpty(x.request) ? (bool?) null : bool.Parse(x.request),
					             IsFavorite = string.IsNullOrEmpty(x.fav) ? (bool?) null : bool.Parse(x.fav),
					             IsMyRequest = string.IsNullOrEmpty(x.mine) ? (bool?) null : bool.Parse(x.mine),
					             SongRating = string.IsNullOrEmpty(x.rating) ? (decimal?) null : decimal.Parse(x.rating)
				             })
				.OrderBy(x => x.SortOrder)
				.ToList();


			Log.Info("Voting priorities loaded.");
			_client.VotePriorities = tempList;
			_client.VotePrioritiesLoaded = File.GetLastWriteTime(Settings.Default.VotingPrefs);
		}

		private void updateRequestQueue(Info infoResult)
		{
			if (infoResult?.Requests == null)
			{
				return;
			}

			_client.RemoveUnavailableRequests();

			if (IsPaused() || infoResult.Requests.Count(x => !x.Cool) >= Settings.Default.MinQueueSize)
			{
				return;
			}

			if (_client.RequestUnratedSongs(_client.CurrentSite))
			{
				Log.Info("Added requests to request queue");
			}
		}
	}
}