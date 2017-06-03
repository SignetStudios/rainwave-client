using System.Collections.Generic;
using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{
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
}