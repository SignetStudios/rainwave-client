using System.Collections.Generic;
using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{
	[JsonObject]
	public class Album
	{
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
	}
}
