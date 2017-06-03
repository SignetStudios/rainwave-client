using System.Collections.Generic;
using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{
	[JsonObject]
	public class ScheduleItem
	{
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
	}
}