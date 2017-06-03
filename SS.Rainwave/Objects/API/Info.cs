using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{
	[JsonObject]
	public class Info
	{
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

		[JsonProperty(PropertyName = "already_voted")]
		private List<List<int>> _voteInfo;

		public List<Vote> AlreadyVoted => _voteInfo.Select(x => new Vote {ElectionId = x[0], SongId = x[1]}).ToList();
	}
}