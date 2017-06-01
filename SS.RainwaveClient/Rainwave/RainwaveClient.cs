using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using SS.RainwaveClient.Rainwave.DataAccess;

namespace SS.RainwaveClient.Rainwave
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
		bool IsPaused();
		Info GetInfo(SiteId siteId, bool checkStation = true);
		bool Vote(SiteId siteId, int entryId);
		Info Sync(SiteId siteId, int knownEventId = -1);
		bool DeleteRequest(int songId, SiteId siteId);
		bool RequestUnratedSongs(SiteId siteId);
		
		#endregion

		#region Advanced Functions

		bool RemoveUnavailableRequests();
		bool AutoVote(Info info);
		bool FixRequestList(Info info);
		void CheckStation();

		#endregion
	}

	public class RainwaveClient : IRainwaveClient
	{
		private readonly IRainwaveApi4 _rainwaveApi;
		private static readonly ILog log = LogManager.GetLogger(typeof(RainwaveClient));
		//private readonly Action<string, int?> _textLogger;
		
		public RainwaveClient(SiteId defaultSite) : this(new RainwaveApi4(), defaultSite)
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

		public bool IsPaused()
		{
			var isPaused = false;

			var info = GetInfo(CurrentSite);

			if (info?.User != null)
				isPaused = info.User.RequestsPaused;

			return isPaused;
		}

		public Info GetInfo(SiteId siteId, bool checkStation = true)
		{
			var info = _rainwaveApi.Info(siteId).Result;

			if (info?.User == null) 
				return info;

			if (checkStation)
			{
				lock (_siteLock)
				{
					CheckStation();
				}
			}

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

			lock(_siteLock)
			{
				CheckStation();
			}

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

		public bool RemoveUnavailableRequests()
		{
			var info = GetInfo(CurrentSite);

			if (info?.Requests == null || !info.Requests.Any())
				return false;

			var cooldownList = info.Requests.Where(x => x.Cool).ToList();

			if (!cooldownList.Any())
				return true;

			foreach (var song in cooldownList.Where(song => DeleteRequest(song.Id, info.User.Sid)))
			{
				log.Debug($"Removed: {song.Title}");
			}

			return true;
		}


		public bool AutoVote(Info info)
		{
			if (info?.SchedNext == null || !info.SchedNext.Any()) return false;
			if (VotePriorities == null || !VotePriorities.Any()) throw new InvalidOperationException("Cannot auto vote while VotePriorities is undefined.");

			foreach (var election in info.SchedNext.Where(x => x.VotingAllowed && info.AlreadyVoted.All(y => y.ElectionId != x.Id)))
			{
				var bestSong = election.Songs
					.Select(song => new
					{
						Song = song,
						Priority =
							VotePriorities.FirstOrDefault(
								x => (x.IsMyRequest == null || x.IsMyRequest == (song.ElecRequestUserId == _rainwaveApi.UserId)) &&
								     (x.IsRequest == null || x.IsRequest == !string.IsNullOrEmpty(song.ElecRequestUsername)) &&
								     (x.SongRating == null || x.SongRating == song.RatingUser) &&
								     (x.IsFavorite == null || x.IsFavorite == song.Fave))
					})
					.Select(x =>
					{
						//This just writes the priority information out as a debug statement. 
						// It can be commented out or removed if desired.
						log.Debug($"{x.Song.Title}: {x.Priority?.SortOrder ?? VotePriorities.Count + 1} ({x.Song.RatingUser})");
						return x;
					})
					.OrderBy(x => x.Priority?.SortOrder ?? int.MaxValue)
					.ThenByDescending(x => x.Song.RatingUser)
					.ThenByDescending(x => x.Song.Albums.OrderByDescending(y => y.RatingUser).FirstOrDefault()?.RatingUser)
					.Select(x => x.Song)
					.First();
				
				if (_rainwaveApi.Vote(CurrentSite, bestSong.EntryId).Result)
					log.Info($"Voted:   {bestSong.Title}", null);
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

		public void CheckStation()
		{
			var currentInfo = GetInfo(CurrentSite, false);

			if (currentInfo.User.TunedIn)
			{
				return;
			}

			//Try the LockSid first so we don't waste unnecessary calls
			if (currentInfo.User.LockSid > 0 && currentInfo.User.LockSid != CurrentSite)
			{
				var lockSidInfo = GetInfo(currentInfo.User.LockSid, false);

				if (lockSidInfo.User.TunedIn)
				{
					CurrentSite = lockSidInfo.User.Sid;
					log.Warn($"Switched stations from {currentInfo.User.Sid} to {CurrentSite}");
					return;
				}

			}

			//We're looking for the site that says the user is logged in to.
			var site = Enum.GetValues(typeof (SiteId))
				.Cast<SiteId>()
				.Select(x => GetInfo(x, false))
				.SingleOrDefault(info => info.User.TunedIn);

			if (site == null)
			{
				return;
			}

			CurrentSite = site.User.Sid;
			log.Warn($"Switched stations from {currentInfo.User.Sid} to {CurrentSite}");
		}

		private class BestSong
		{
			public Song Song { get; set; }
			public int Ranking { get; set; }
		}
	}
}
