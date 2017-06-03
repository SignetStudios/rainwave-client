using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{
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
}