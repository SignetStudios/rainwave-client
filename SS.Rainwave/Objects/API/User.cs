using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{
	[JsonObject]
	public class User
	{
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
	}
}