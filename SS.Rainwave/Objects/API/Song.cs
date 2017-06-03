using System.Collections.Generic;
using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{

	[JsonObject]
	public class Song
	{
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
	}
}
