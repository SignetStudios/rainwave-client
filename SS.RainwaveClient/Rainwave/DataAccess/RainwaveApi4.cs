using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SS.RainwaveClient.Properties;

namespace SS.RainwaveClient.Rainwave.DataAccess
{
	public class RainwaveApi4 : IRainwaveApi4
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(RainwaveApi4));

		public RainwaveApi4()
			: this(Settings.Default.BaseApiUrl, Settings.Default.UserId, Settings.Default.ApiKey)
		{
		}

		internal RainwaveApi4(string apiEndpoint, string userId, string apiKey)
		{
			ApiEndpoint = apiEndpoint;
			UserId = userId;
			ApiKey = apiKey;
		}


		public string ApiEndpoint { get; }

		public string UserId { get; }

		public string ApiKey { get; }

		public async Task<Album> Album(SiteId siteId, int albumId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"id", albumId.ToString(CultureInfo.InvariantCulture)}
			            };

			try
			{
				var result = await makeRequest<Album>("album", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Album>> AllAlbums(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Album>>("all_albums", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Artist>> AllArtists(SiteId siteId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			try
			{
				var result = await makeRequest<List<Artist>>("all_artists", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Song>> AllFaves(int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"per_page", perPage.ToString(CultureInfo.InvariantCulture)},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Song>>("all_faves", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Group>> AllGroups(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Group>>("all_groups", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Song>> AllSongs(Order sortOrder, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"order", ((int) sortOrder).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Song>>("all_songs", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Artist> Artist(SiteId siteId, int artistId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"id", artistId.ToString(CultureInfo.InvariantCulture)}
			            };

			try
			{
				var result = await makeRequest<Artist>("artist", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> ClearRating(SiteId siteId, int songId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"song_id", songId.ToString(CultureInfo.InvariantCulture)},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("clear_rating", parms);
		}

		public async Task<bool> ClearRequests()
		{
			var result = await makeRequest("clear_requests");

			try
			{
				var execTime = result["api_info"]["exectime"].Value<decimal>();

				return execTime > 0;
			}
			catch
			{
				return false;
			}
		}

		public async Task<List<User>> CurrentListeners(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<User>>("current_listeners", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> DeleteRequest(int songId, SiteId siteId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"song_id", songId.ToString(CultureInfo.InvariantCulture)},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("delete_request", parms);
		}

		public async Task<bool> FaveAlbum(SiteId siteId, int albumId, bool fave)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"album_id", albumId.ToString(CultureInfo.InvariantCulture)},
				            {"fave", fave.ToString()},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("fave_album", parms);
		}

		public async Task<bool> FaveSong(SiteId siteId, int songId, bool fave)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"song_id", songId.ToString(CultureInfo.InvariantCulture)},
				            {"fave", fave.ToString().ToLower()},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("fave_song", parms);
		}

		public async Task<Group> Group(SiteId siteId, int groupId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"id", groupId.ToString(CultureInfo.InvariantCulture)}
			            };

			try
			{
				var result = await makeRequest<Group>("group", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Info> Info(SiteId siteId, bool currentListeners = false, bool allAlbums = false)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"current_listeners", currentListeners.ToString().ToLower()},
				            {"all_albums", allAlbums.ToString().ToLower()}
			            };

			try
			{
				var result = await makeRequest<Info>("info", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Info>> InfoAll(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Info>>("info_all", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<User> Listener(int id)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"id", id.ToString()}
			            };

			try
			{
				var result = await makeRequest<User>("listener", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> OrderRequests(string songIdList, SiteId siteId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"order", songIdList},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("order_requests", parms);
		}

		public async Task<bool> PauseRequestQueue(SiteId siteId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("pause_request_queue", parms);
		}

		public async Task<List<Song>> PlaybackHistory(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Song>>("playback_history", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> Rate(SiteId siteId, int songId, decimal rating)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"song_id", songId.ToString(CultureInfo.InvariantCulture)},
				            {"rating", rating.ToString(CultureInfo.InvariantCulture)},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("rate", parms);
		}

		public async Task<bool> Request(SiteId siteId, int songId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"song_id", songId.ToString(CultureInfo.InvariantCulture)},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("request", parms);
		}

		public async Task<bool> RequestFavoritedSongs(SiteId siteId, int limit = int.MaxValue)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"limit", limit.ToString(CultureInfo.InvariantCulture)},
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("request_favorited_songs", parms);
		}

		public async Task<List<Request>> RequestLine(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Request>>("request_line", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> RequestUnratedSongs(SiteId siteId, int limit = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };

			if (limit > 0) parms.Add("limit", limit.ToString(CultureInfo.InvariantCulture));

			return await getRequestSuccess("request_unrated_songs", parms);
		}

		public async Task<Search> Search(SiteId siteId, string searchString)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"search", searchString}
			            };

			try
			{
				var result = await makeRequest<Search>("artist", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Song> Song(SiteId siteId, int songId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"id", songId.ToString(CultureInfo.InvariantCulture)}
			            };

			try
			{
				var result = await makeRequest<Song>("song", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Dictionary<SiteId, int>> StationSongCount()
		{
			try
			{
				var rets = await makeRequest<List<dynamic>>("station_song_count");

				return rets.ToDictionary<dynamic, SiteId, int>(ret => ret.sid, ret => ret.song_count);
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Station>> Stations(int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Station>>("stations", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Info> Sync(SiteId siteId, bool offlineAck = false, bool resync = false, int knownEventId = -1)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"offline_ack", offlineAck.ToString()},
				            {"resync", resync.ToString()}
			            };

			if (knownEventId > 0)
			{
				parms.Add("known_event_id", knownEventId.ToString(CultureInfo.InvariantCulture));
			}

			try
			{
				var result = await makeRequest<Info>("sync", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Donation>> TipJar(int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Donation>>("tip_jar", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Song>> Top100(int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Song>>("top_100", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Song>> UnratedSongs(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Song>>("unrated_songs", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> UnpauseRequestQueue(SiteId siteId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)}
			            };
			return await getRequestSuccess("unpause_request_queue", parms);
		}

		public async Task<User> UserInfo(int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<User>("user_info", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Song>> UserRecentVotes(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Song>>("user_recent_votes", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<Song>> UserRequestedHistory(SiteId siteId, int perPage = 0, int pageStart = 0)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"per_page", perPage.ToString()},
				            {"page_start", pageStart.ToString()}
			            };

			try
			{
				var result = await makeRequest<List<Song>>("user_requested_history", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<User>> UserSearch(string userName)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"username", userName}
			            };

			try
			{
				var result = await makeRequest<List<User>>("user_search", parms);

				return result;
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> Vote(SiteId siteId, int entryId)
		{
			var parms = new Dictionary<string, string>
			            {
				            {"sid", ((int) siteId).ToString(CultureInfo.InvariantCulture)},
				            {"entry_id", entryId.ToString(CultureInfo.InvariantCulture)}
			            };

			return await getRequestSuccess("vote", parms);
		}

		private async Task<bool> getRequestSuccess(string url, Dictionary<string, string> additionalValues = null)
		{
			var result = await makeRequest(url, additionalValues);

			try
			{
				var success = result[url + "_result"]["success"].Value<bool>();

				return success;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		///     Makes a request to the API endpoint and returns the response
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url">The specific URL endpoint to send the response to</param>
		/// <param name="additionalValues">Any additional parameters that should be passed with the request</param>
		/// <returns>An object T containing the response</returns>
		private async Task<T> makeRequest<T>(string url, Dictionary<string, string> additionalValues = null)
			where T : class
		{
			var attempt = 1;

			var parms = new NameValueCollection
			            {
				            {"user_id", UserId},
				            {"key", ApiKey}
			            };

			if (additionalValues != null)
			{
				foreach (var value in additionalValues)
				{
					parms.Add(value.Key, value.Value);
				}
			}

			while (attempt <= 3)
			{
				using (var client = new WebClient())
				{
					try
					{
						var response =
							await client.UploadValuesTaskAsync($"{ApiEndpoint}/{url}", parms);

						var responseString = Encoding.Default.GetString(response);
						var returnVal = JsonConvert.DeserializeObject<T>(responseString, new JsonSerializerSettings
						                                                                 {
							                                                                 NullValueHandling = NullValueHandling.Ignore
						                                                                 });

						return returnVal;
					}
					catch (WebException e)
					{
						if (e.Status == WebExceptionStatus.ProtocolError)
						{
							var statusCode = ((HttpWebResponse) e.Response).StatusCode;
							switch (statusCode)
							{
								case HttpStatusCode.BadGateway:
									//suppress BadGateway
									break;
								default:
									Log.Error($"Error with {url} ({statusCode})");
									break;
							}
						}
						else
						{
							Log.Error($"Error processing request: {url}");
						}
					}
				}

				attempt++;
			}

			return null;
		}

		/// <summary>
		///     Makes a request to the API endpoint and returns the response
		/// </summary>
		/// <param name="url">The specific URL endpoint to send the response to</param>
		/// <param name="additionalValues">Any additional parameters that should be passed with the request</param>
		/// <returns>A dynamic object representing the JSON result</returns>
		private async Task<JObject> makeRequest(string url, Dictionary<string, string> additionalValues = null)
		{
			return await makeRequest<JObject>(url, additionalValues);
		}
	}
}