using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SS.RainwaveClient.Properties;
using SS.RainwaveClient.Rainwave;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using log4net;

namespace SS.RainwaveClient
{
	public class Program
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (Program));

		#region Public Static Members

		public static void Main()
		{
			Console.SetWindowSize(75, 30);

			var work = new Workhorse();

			log.Info("Starting RW Interface.");
			log.Info("Input anything to stop the process and clear the queue.");

			ThreadPool.QueueUserWorkItem(work.ExecuteWork);

			var curDateTime = DateTime.Now;
			var curTime = curDateTime.TimeOfDay;

			var pauseTime = Settings.Default.AutoPauseTime - curTime;
			var unpauseTime = Settings.Default.AutoUnpauseTime - curTime;

			pauseTime += pauseTime.TotalSeconds < 0 ? new TimeSpan(1, 0, 0, 0) : new TimeSpan(0); //Adjust to next runtime
			unpauseTime += unpauseTime.TotalSeconds < 0 ? new TimeSpan(1, 0, 0, 0) : new TimeSpan(0); //Adjust to next runtime
			
			var autoPause = new Timer(work.AutoPauseRequestQueue, null, pauseTime, new TimeSpan(1, 0, 0, 0));
			var autoUnPause = new Timer(work.AutoUnpauseRequestQueue, null, unpauseTime, new TimeSpan(1, 0, 0, 0));

			log.Info($"Auto pause delayed until {curDateTime.Add(pauseTime)}.");
			log.Info($"Auto unpause delayed until {curDateTime.Add(unpauseTime)}.");

			string con;
			var isPaused = work.IsPaused();

			log.Info($"Requests currently {(isPaused ? "" : "un")}paused.");

			do
			{
				con = Console.ReadLine();

				if (!string.IsNullOrWhiteSpace(con)) break;

				isPaused = work.IsPaused();
				
				if (isPaused) work.UnpauseRequestQueue();
				if (!isPaused) work.PauseRequestQueue();

				log.Info($"Requests {(!isPaused ? "" : "un")}paused");

			} while (string.IsNullOrWhiteSpace(con));

			work.StopWork = true;
			work.AutoPauseRequestQueue(null);
		}

		#endregion		
	}

	internal class Workhorse
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Workhorse));

		private readonly IRainwaveClient _client;
		public bool StopWork { get; set; }
	

		public Workhorse() : this(new Rainwave.RainwaveClient((SiteId) Settings.Default.DefaultStation))
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
			if (IsPaused()) return;

			PauseRequestQueue();
			log.Info("Request queue auto paused.");
		}

		public void PauseRequestQueue()
		{
			_client.PauseRequestQueue(_client.CurrentSite);
		}

		public void AutoUnpauseRequestQueue(object timerObj)
		{
			if (!IsPaused()) return;

			UnpauseRequestQueue();
			log.Info("Request queue auto unpaused.");
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

			UpdateRequestQueue(info);
			Vote(_client.GetInfo(_client.CurrentSite));

			while (!StopWork)
			{
				var sync = _client.Sync(_client.CurrentSite);

				if (sync?.User == null) continue;

				if (!IsPaused() && !sync.User.TunedIn)
				{
					AutoPauseRequestQueue(null);
				}

				if (IsPaused()) continue;

				if (sync.SchedNext == null || !sync.SchedNext.Any()) continue;

				UpdateRequestQueue(sync);

				if (sync.User.TunedIn)
					Vote(sync);
			}
		}

		private void Vote(Info info)
		{
			LoadVotePriorities();

			_client.AutoVote(info);
		}

		private void LoadVotePriorities()
		{
			if (_client.VotePriorities != null &&
			    File.GetLastWriteTime(Settings.Default.VotingPrefs) <= _client.VotePrioritiesLoaded)
			{
				return;
			}

			var fileContents = XDocument.Load(Settings.Default.VotingPrefs);

			if (fileContents.Root == null)
				return;
				
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


			log.Info("Voting priorities loaded.");
			_client.VotePriorities = tempList;
			_client.VotePrioritiesLoaded = File.GetLastWriteTime(Settings.Default.VotingPrefs);
		}

		private void UpdateRequestQueue(Info infoResult)
		{
			if (infoResult?.Requests == null) return;

			_client.RemoveUnavailableRequests();

			if (IsPaused() || infoResult.Requests.Count(x => !x.Cool) >= Settings.Default.MinQueueSize) return;

			if (_client.RequestUnratedSongs(_client.CurrentSite))
				log.Info("Added requests to request queue");
		}
	}
}