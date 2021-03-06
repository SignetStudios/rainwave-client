﻿using System;
using System.CodeDom;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using log4net;
using SS.Rainwave.Client.Console.Properties;
using SS.Rainwave.Objects;
using SS.Rainwave.Objects.API;
using Windows.UI.Notifications;

namespace SS.Rainwave.Client.Console
{
	public class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
		private static Timer _autoPause;
		private static Timer _autoUnPause;

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

			pauseTime +=
				pauseTime.TotalSeconds < 0 ? new TimeSpan(1, 0, 0, 0) : new TimeSpan(0); //Adjust to next runtime
			unpauseTime += unpauseTime.TotalSeconds < 0
				? new TimeSpan(1, 0, 0, 0)
				: new TimeSpan(0); //Adjust to next runtime

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
	}

	internal class Workhorse
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Workhorse));

		private readonly IRainwaveClient _client;
		private ToastNotifier _toastNotifier;

		public bool StopWork { get; set; }


		public Workhorse() : this(new RainwaveClient(Settings.Default.BaseApiUrl, Settings.Default.UserId,
			Settings.Default.ApiKey, (SiteId)Settings.Default.DefaultStation))
		{
		}

		internal Workhorse(IRainwaveClient rainwaveClient)
		{
			_client = rainwaveClient;
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
			Vote(_client.GetInfo(_client.CurrentSite));
		}

		public bool IsPaused()
		{
			return _client.IsPaused();
		}

		private void ToastSong(Info songInfo, bool withImage = false)
		{
			if (_toastNotifier == null)
			{
				_toastNotifier = ToastNotificationManager.CreateToastNotifier("SS.Rainwave.Client.Console");
			}

			var notification = ToastNotificationManager.GetTemplateContent(withImage
				? ToastTemplateType.ToastImageAndText04
				: ToastTemplateType.ToastText04);

			if (songInfo?.SchedCurrent?.Songs == null || songInfo.SchedCurrent.Songs.Count == 0)
			{
				return;
			}

			var currentSong = songInfo.SchedCurrent.Songs[0];

			if (withImage)
			{
				var image = $"http://www.rainwave.cc{currentSong.Albums[0].Art}_120.jpg";

				var notificationImage = (XmlElement)notification.GetElementsByTagName("image")[0];
				notificationImage.SetAttribute("src", image);
				notificationImage.SetAttribute("alt", currentSong.Albums[0].Name);
			}

			var notificationLines = notification.GetElementsByTagName("text");

			notificationLines[0].InnerText =
				$"Now Playing: {currentSong.Title} ({(currentSong.RatingUser == 0 ? "Unrated" : $"Rated {currentSong.RatingUser:#.0}")})";
			notificationLines[1].InnerText = $"Album: {currentSong.Albums[0].Name}";

			if (!string.IsNullOrWhiteSpace(currentSong.ElecRequestUsername))
			{
				notificationLines[2].InnerText = $"Requested by: {currentSong.ElecRequestUsername}";
			}

			var toastNotification = new ToastNotification(notification)
			{
				ExpirationTime = DateTimeOffset.Now.AddSeconds(currentSong.Length)
			};

			_toastNotifier.Show(toastNotification);

		}

		public void ExecuteWork(object state)
		{
			var info = _client.GetInfo(_client.CurrentSite);

			ToastSong(info);

			UpdateRequestQueue(info);
			Vote(_client.GetInfo(_client.CurrentSite));

			while (!StopWork)
			{
				var sync = _client.Sync(_client.CurrentSite);

				if (sync?.User == null)
				{
					continue;
				}

				ToastSong(sync);

				if (!IsPaused() && !sync.User.TunedIn)
				{
					AutoPauseRequestQueue(null);
				}

				if (IsPaused())
				{
					continue;
				}


				if (sync.SchedNext == null || !sync.SchedNext.Any())
				{
					continue;
				}

				UpdateRequestQueue(sync);

				if (sync.User.TunedIn)
				{
					Vote(sync);
				}
			}
		}

		private void Vote(Info info)
		{
			LoadVotePriorities();

			_client.AutoVote(info);
		}

		private T ParseElement<T>(XElement element)
		{
			bool IsNullable()
			{
				return Nullable.GetUnderlyingType(typeof(T)) != null;
			}

			if (element == null)
			{
				if (!IsNullable())
				{
					//Type is not nullable, but element is null. 
					throw new InvalidOperationException();
				}

				return default(T);
			}

			var converter = TypeDescriptor.GetConverter(typeof(T));

			if (converter.IsValid(element.Value))
			{
				return (T)converter.ConvertFromString(element.Value);
			}

			if (!IsNullable())
			{
				//Type is not nullable, but element is null. 
				throw new InvalidOperationException();
			}

			return default(T);
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
			{
				return;
			}

			var tempList = fileContents.Root.Elements()
				.Select(x => new VotePriority
				{
					SortOrder = ParseElement<int>(x?.Element("SortOrder")),
					IsRequest = ParseElement<bool?>(x?.Element("IsRequest")),
					IsFavorite = ParseElement<bool?>(x?.Element("IsFavorite")),
					IsMyRequest = ParseElement<bool?>(x?.Element("IsMyRequest")),
					SongRating = ParseElement<decimal?>(x?.Element("SongRating"))
				}).OrderBy(x => x.SortOrder)
				.ToList();


			Log.Info("Voting priorities loaded.");
			_client.VotePriorities = tempList;
			_client.VotePrioritiesLoaded = File.GetLastWriteTime(Settings.Default.VotingPrefs);
		}

		private void UpdateRequestQueue(Info infoResult)
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