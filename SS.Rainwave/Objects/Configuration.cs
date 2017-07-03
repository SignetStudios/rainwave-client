using System;
using System.Collections.Generic;
using SS.Rainwave.API.Objects;

namespace SS.Rainwave.Objects
{
	public class Configuration
	{
		public int UserId { get; set; }
		public string BaseApiUrl { get; set; }
		public TimeSpan RecheckTime { get; set; }
		public TimeSpan RequestTimeout { get; set; }
		public int MinQueueSize { get; set; }
		public TimeSpan AutoPauseTime { get; set; }
		public TimeSpan AutoUnpauseTime { get; set; }
		public SiteId DefaultStation { get; set; }
		public string ApiKey { get; set; }
		public List<VotePriority> VotingPreferences { get; set; }
	}
}