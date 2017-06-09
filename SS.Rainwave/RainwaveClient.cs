using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using SS.Rainwave.API;
using SS.Rainwave.API.Objects;
using SS.Rainwave.Objects;

namespace SS.Rainwave
{
	public interface IRainwaveClient
	{
		#region Properties

		SiteId CurrentSite { get; }
		List<VotePriority> VotePriorities { get; set; }
		DateTime VotePrioritiesLoaded { get; set; }

		#endregion

		#region Basic Functions

		bool ClearRequestQueue();
		bool PauseRequestQueue(SiteId siteId);
		bool UnpauseRequestQueue(SiteId siteId);
		bool IsPaused(Info info = null);
		Info GetInfo(SiteId siteId, bool checkStation = true);
		bool Vote(SiteId siteId, int entryId);
		Info Sync(SiteId siteId, int knownEventId = -1);
		bool DeleteRequest(int songId, SiteId siteId);
		bool RequestUnratedSongs(SiteId siteId);

		#endregion

		#region Advanced Functions

		bool RemoveUnavailableRequests(Info info = null);
		bool AutoVote(Info info);
		bool FixRequestList(Info info);
		void CheckStation(Info info = null);

		#endregion
	}

	public class RainwaveClient : IRainwaveClient
	{
		private readonly IRainwaveApi4 _rainwaveApi;

		private static readonly ILog Log = LogManager.GetLogger(typeof(RainwaveClient));

		public RainwaveClient(string apiEndpoint, string userId, string apiKey, SiteId defaultSite)
			: this(new RainwaveApi4(apiEndpoint, userId, apiKey, LogManager.GetLogger(typeof(RainwaveApi4))), defaultSite)
		{
		}

		internal RainwaveClient(IRainwaveApi4 rainwaveApi, SiteId currentSite)
		{
			_rainwaveApi = rainwaveApi;
			CurrentSite = currentSite;
		}

		public SiteId CurrentSite { get; private set; }
		private readonly object _siteLock = new object();
		public List<VotePriority> VotePriorities { get; set; }

		public DateTime VotePrioritiesLoaded { get; set; }

		public bool ClearRequestQueue()
		{
			return _rainwaveApi.ClearRequests().Result;
		}

		public bool PauseRequestQueue(SiteId siteId)
		{
			return _rainwaveApi.PauseRequestQueue(siteId).Result;
		}

		public bool UnpauseRequestQueue(SiteId siteId)
		{
			return _rainwaveApi.UnpauseRequestQueue(siteId).Result;
		}

		public bool IsPaused(Info info = null)
		{
			var isPaused = false;

			var currentInfo = info ?? GetInfo(CurrentSite);

			if (currentInfo?.User != null)
				isPaused = currentInfo.User.RequestsPaused;

			return isPaused;
		}

		public Info GetInfo(SiteId siteId, bool checkStation = true)
		{
			var info = _rainwaveApi.Info(siteId).Result;

			if (info?.User == null)
				return info;

			if (!checkStation)
			{
				return info;
			}

			CheckStation(info);

			if (siteId == CurrentSite)
			{
				return info;
			}

			info = _rainwaveApi.Info(CurrentSite).Result;

			return info;
		}

		public bool Vote(SiteId siteId, int entryId)
		{
			return _rainwaveApi.Vote(siteId, entryId).Result;
		}

		public Info Sync(SiteId siteId, int knownEventId = -1)
		{
			var info = _rainwaveApi.Sync(siteId, knownEventId: knownEventId).Result;

			if (info?.User == null)
				return info;

			CheckStation(info);

			return info;
		}

		public bool DeleteRequest(int songId, SiteId siteId)
		{
			return _rainwaveApi.DeleteRequest(songId, siteId).Result;
		}

		public bool RequestUnratedSongs(SiteId siteId)
		{
			return _rainwaveApi.RequestUnratedSongs(siteId).Result;
		}

		public bool RemoveUnavailableRequests(Info info = null)
		{
			var currentInfo = info ?? GetInfo(CurrentSite);

			if (currentInfo?.Requests == null || !currentInfo.Requests.Any())
				return false;

			var cooldownList = currentInfo.Requests.Where(x => x.Cool).ToList();

			if (!cooldownList.Any())
				return true;

			foreach (var song in cooldownList.Where(song => DeleteRequest(song.Id, currentInfo.User.Sid)))
			{
				Log.Debug($"Removed: {song.Title}");
			}

			return true;
		}


		public bool AutoVote(Info info)
		{
			if (info?.SchedNext == null || !info.SchedNext.Any()) return false;
			if (VotePriorities == null || !VotePriorities.Any())
				throw new InvalidOperationException("Cannot auto vote while VotePriorities is undefined.");

			foreach (var election in info.SchedNext.Where(x => x.VotingAllowed &&
			                                                   info.AlreadyVoted.All(y => y.ElectionId != x.Id)))
			{
				var bestSong = election.Songs
					.Select(song => new
					{
						Song = song,
						Priority =
						VotePriorities.FirstOrDefault(
							x => (x.IsMyRequest == null || x.IsMyRequest ==
							      (song.ElecRequestUserId == _rainwaveApi.UserId)) &&
							     (x.IsRequest == null || x.IsRequest == !string.IsNullOrEmpty(song.ElecRequestUsername)) &&
							     (x.SongRating == null || x.SongRating == song.RatingUser) &&
							     (x.IsFavorite == null || x.IsFavorite == song.Fave))
					})
					.Select(x =>
					{
						//This just writes the priority information out as a debug statement. 
						// It can be commented out or removed if desired.
						Log.Debug($"{x.Song.Title}: {x.Priority?.SortOrder ?? VotePriorities.Count + 1} ({x.Song.RatingUser})");
						return x;
					})
					.OrderBy(x => x.Priority?.SortOrder ?? int.MaxValue)
					.ThenByDescending(x => x.Song.RatingUser)
					.ThenByDescending(x => x.Song.Albums.OrderByDescending(y => y.RatingUser).FirstOrDefault()?.RatingUser)
					.Select(x => x.Song)
					.First();

				if (_rainwaveApi.Vote(CurrentSite, bestSong.EntryId).Result)
					Log.Info($"Voted:   {bestSong.Title}", null);
			}

			return true;
		}

		public bool FixRequestList(Info info)
		{
			if (info?.Requests == null) return false;

			var requestList = info.Requests;

			if (requestList.Count == 0) return true;

			var requestIds = requestList.Select(x => x.Id.ToString(CultureInfo.InvariantCulture)).ToList();

			var requestOrder = requestIds.Aggregate((a, b) => a + "," + b);

			return _rainwaveApi.OrderRequests(requestOrder, info.User.Sid).Result;
		}

		public void CheckStation(Info info = null)
		{
			lock (_siteLock)
			{
				var currentInfo = info ?? GetInfo(CurrentSite, false);

				if (currentInfo.User.TunedIn)
				{
					return;
				}

				if (currentInfo.User.LockSid == CurrentSite)
				{
					return;
				}

				//Try the LockSid first so we don't waste unnecessary calls
				if (currentInfo.User.LockSid > 0)
				{
					var lockSidInfo = GetInfo(currentInfo.User.LockSid, false);

					if (lockSidInfo.User.TunedIn)
					{
						CurrentSite = lockSidInfo.User.Sid;
						Log.Warn($"Switched stations from {currentInfo.User.Sid} to {CurrentSite}");
						return;
					}
				}

				//We're looking for the site that says the user is listening in to.
				var site = Enum.GetValues(typeof(SiteId))
					.Cast<SiteId>()
					.Select(x => GetInfo(x, false))
					.SingleOrDefault(x => x.User.TunedIn);

				if (site == null)
				{
					return;
				}

				CurrentSite = site.User.Sid;
				Log.Warn($"Switched stations from {currentInfo.User.Sid} to {CurrentSite}");
			}
		}
	}
}