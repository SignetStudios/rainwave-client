using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
				RequestTimeout = Settings.Default.RequestTimeout
			};

			var fileContents = XDocument.Load(Settings.Default.VotingPrefs);

			if (fileContents.Root != null)
			{
				config.VotingPreferences = fileContents.Root.Elements()
						.Select(x => new VotePriority
						             {
							             SortOrder = ParseElement<int>(x?.Element("SortOrder")),
							             IsRequest = ParseElement<bool?>(x?.Element("IsRequest")),
							             IsFavorite = ParseElement<bool?>(x?.Element("IsFavorite")),
							             IsMyRequest = ParseElement<bool?>(x?.Element("IsMyRequest")),
							             SongRating = ParseElement<decimal?>(x?.Element("SongRating"))
						             }).OrderBy(x => x.SortOrder)
						.ToList();
			}
			
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

		private static T ParseElement<T>(XElement element)
		{
			bool IsNullable()
			{
				return Nullable.GetUnderlyingType(typeof(T)) != null;
			}

			if (element == null)
			{
				if (!IsNullable())
				{
					//Type is not nullable, but element is null. 
					throw new InvalidOperationException();
				}

				return default(T);
			}

			var converter = TypeDescriptor.GetConverter(typeof(T));

			if (converter.IsValid(element.Value))
			{
				return (T)converter.ConvertFromString(element.Value);
			}

			if (!IsNullable())
			{
				//Type is not nullable, but element is null. 
				throw new InvalidOperationException();
			}

			return default(T);
		}
	}
}
