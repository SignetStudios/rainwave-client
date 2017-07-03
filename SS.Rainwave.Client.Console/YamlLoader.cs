using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using SS.Rainwave.API.Objects;
using SS.Rainwave.Client.Console.Properties;
using SS.Rainwave.Objects;
using YamlDotNet.Serialization;

namespace SS.Rainwave.Client.Console
{
	public static class YamlLoader
	{
		public static Configuration LoadConfiguration(string path = "rainwave.yaml")
		{
			try
			{
				using (var reader = File.OpenText(path))
				{
					var deserializer = new Deserializer();
					var config = deserializer.Deserialize<Configuration>(reader);

					return config;
				}
			}
			catch
			{
				LogManager.GetLogger(typeof(RainwaveClient)).Error("Could not load YAML configuration");
				return null;
			}
		}

		public static void ConvertConfiguration(string path = "rainwave.yaml")
		{
			var config = new Configuration
			{
				BaseApiUrl = Settings.Default.BaseApiUrl,
				DefaultStation = (SiteId)Settings.Default.DefaultStation,
				ApiKey = Settings.Default.ApiKey,
				UserId = Settings.Default.UserId,
				AutoPauseTime = Settings.Default.AutoPauseTime,
				AutoUnpauseTime = Settings.Default.AutoUnpauseTime,
				MinQueueSize = Settings.Default.MinQueueSize,
				RecheckTime = Settings.Default.RecheckTime,
				RequestTimeout = Settings.Default.RequestTimeout,
				VotingPreferences = new List<VotePriority>
				{
					new VotePriority
					{
						SortOrder = 1,
						IsRequest = true
					}
				}
			};

			try
			{
				using (var writer = new StreamWriter(path))
				{
					var serializer = new Serializer();

					serializer.Serialize(writer, config);
				}
			}
			catch
			{
				LogManager.GetLogger(typeof(RainwaveClient)).Error("Could not convert to YAML configuration");
			}
		}
	}
}
