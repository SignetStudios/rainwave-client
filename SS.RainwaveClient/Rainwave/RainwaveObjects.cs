using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SS.RainwaveClient.Rainwave
{
	public enum Order
	{
		None = 0,
		Name = 1,
		Rating = 2
	}

	public enum SiteId
	{
		Game = 1,
		OCRemix = 2,
		Covers = 3,
		Chiptunes = 4,
		All = 5
	}

	[JsonObject]
	public class Song
	{
		#region Properties

		[JsonProperty(PropertyName = "albums")]
		public List<Album> Albums { get; set; }

		[JsonProperty(PropertyName = "artists")]
		public List<Artist> Artists { get; set; }

		[JsonProperty(PropertyName = "cool")]
		public bool Cool { get; set; }

		[JsonProperty(PropertyName = "cool_end")]
		public int CoolEnd { get; set; }

		[JsonProperty(PropertyName = "elec_blocked")]
		public bool ElecBlocked { get; set; }

		[JsonProperty(PropertyName = "elec_request_user_id")]
		public string ElecRequestUserId { get; set; }

		[JsonProperty(PropertyName = "elec_request_username")]
		public string ElecRequestUsername { get; set; }

		[JsonProperty(PropertyName = "entry_id")]
		public int EntryId { get; set; }

		[JsonProperty(PropertyName = "entry_type")]
		public int EntryType { get; set; }

		[JsonProperty(PropertyName = "entry_votes")]
		public int EntryVotes { get; set; }

		[JsonProperty(PropertyName = "exists")]
		public bool Exists { get; set; }

		[JsonProperty(PropertyName = "fave")]
		public bool Fave { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "length")]
		public int Length { get; set; }

		[JsonProperty(PropertyName = "rating_user")]
		public decimal RatingUser { get; set; }

		[JsonProperty(PropertyName = "sids")]
		public List<int> Sids { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		#endregion
	}

	[JsonObject]
	public class Album
	{
		#region Properties

		[JsonProperty(PropertyName = "art")]
		public string Art { get; set; }

		[JsonProperty(PropertyName = "fave")]
		public bool Fave { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "rating")]
		public decimal Rating { get; set; }

		[JsonProperty(PropertyName = "rating_complete")]
		public bool RatingComplete { get; set; }

		[JsonProperty(PropertyName = "rating_user")]
		public decimal RatingUser { get; set; }

		[JsonProperty(PropertyName = "songs")]
		public List<Song> Songs { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		#endregion
	}

	[JsonObject]
	public class Artist
	{
		#region Properties

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "name_searchable")]
		public string NameSearchable { get; set; }

		#endregion
	}

	[JsonObject]
	public class Station
	{
		#region Properties

		[JsonProperty(PropertyName = "id")]
		public SiteId Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "stream")]
		public string Stream { get; set; }

		#endregion
	}

	[JsonObject]
	public class Listeners
	{
		#region Properties

		[JsonProperty(PropertyName = "guests")]
		public int Guests { get; set; }

		[JsonProperty(PropertyName = "users")]
		public List<User> Users { get; set; }

		#endregion
	}

	[JsonObject]
	public class User
	{
		#region Properties

		[JsonProperty(PropertyName = "lock")]
		public bool Lock { get; set; }

		[JsonProperty(PropertyName = "admin")]
		public bool Admin { get; set; }

		[JsonProperty(PropertyName = "avatar")]
		public string Avatar { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "listener_id")]
		public int ListenerId { get; set; }

		[JsonProperty(PropertyName = "lock_sid")]
		public SiteId LockSid { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "sid")]
		public SiteId Sid { get; set; }

		[JsonProperty(PropertyName = "tuned_in")]
		public bool TunedIn { get; set; }

		[JsonProperty(PropertyName = "requests_paused")]
		public bool RequestsPaused { get; set; }

		#endregion
	}

	[JsonObject]
	public class Info
	{
		#region Properties

		[JsonProperty(PropertyName = "album_diff")]
		public List<Album> AlbumDiff { get; set; }

		[JsonProperty(PropertyName = "requests")]
		public List<Song> Requests { get; set; }

		[JsonProperty(PropertyName = "sched_current")]
		public ScheduleItem SchedCurrent { get; set; }

		[JsonProperty(PropertyName = "sched_history")]
		public List<ScheduleItem> SchedHistory { get; set; }

		[JsonProperty(PropertyName = "sched_next")]
		public List<ScheduleItem> SchedNext { get; set; }

		[JsonProperty(PropertyName = "user")]
		public User User { get; set; }

		[JsonProperty(PropertyName = "already_voted")] private List<List<int>> _voteInfo;

		public List<Vote> AlreadyVoted => _voteInfo.Select(x => new Vote {ElectionId = x[0], SongId = x[1]}).ToList();

		#endregion
	}

	public class Vote
	{
		public int ElectionId { get; set; }
		public int SongId { get; set; }
	}

	[JsonObject]
	public class ScheduleItem
	{
		#region Properties

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "sid")]
		public SiteId Sid { get; set; }

		[JsonProperty(PropertyName = "songs")]
		public List<Song> Songs { get; set; }

		[JsonProperty(PropertyName = "start")]
		public decimal Start { get; set; }

		[JsonProperty(PropertyName = "start_actual")]
		public int StartActual { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "used")]
		public bool Used { get; set; }

		[JsonProperty(PropertyName = "voting_allowed")]
		public bool VotingAllowed { get; set; }

		#endregion
	}

	[JsonObject]
	public class Group
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "name_searchable")]
		public string NameSearchable { get; set; }

		[JsonProperty(PropertyName = "all_songs_for_sid")]
		public List<Song> Songs { get; set; }
	}

	[JsonObject]
	public class Request
	{
		[JsonProperty(PropertyName = "username")]
		public string UserName { get; set; }

		[JsonProperty(PropertyName = "user_id")]
		public int UserId { get; set; }

		[JsonProperty(PropertyName = "song_id")]
		public int SongId { get; set; }

		[JsonProperty(PropertyName = "song")]
		public Song Song { get; set; }

		[JsonProperty(PropertyName = "skip")]
		public bool Skip { get; set; }

		[JsonProperty(PropertyName = "position")]
		public int Position { get; set; }
	}

	[JsonObject]
	public class Search
	{
		[JsonProperty(PropertyName = "albums")]
		public List<Album> Albums { get; set; }

		[JsonProperty(PropertyName = "songs")]
		public List<Song> Songs { get; set; }

		[JsonProperty(PropertyName = "artists")]
		public List<Artist> Artists { get; set; }
	}

	[JsonObject]
	public class Donation
	{
		[JsonProperty(PropertyName = "amount")]
		public decimal Amount { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
	}
}