using Newtonsoft.Json;

namespace SS.Rainwave.Objects.API
{
	[JsonObject]
	public class Station
	{
		[JsonProperty(PropertyName = "id")]
		public SiteId Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "stream")]
		public string Stream { get; set; }
	}
}