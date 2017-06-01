using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SS.RainwaveClient.Rainwave.DataAccess
{
	public interface IRainwaveApi4
	{
		string ApiEndpoint { get; }
		string UserId { get; }
		string ApiKey { get; }

		Task<Album> Album(SiteId siteId, int albumId);
		Task<List<Album>> AllAlbums(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<List<Artist>> AllArtists(SiteId siteId);
		Task<List<Song>> AllFaves(int perPage = 0, int pageStart = 0);
		Task<List<Group>> AllGroups(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<List<Song>> AllSongs(Order sortOrder, int perPage = 0, int pageStart = 0);
		Task<Artist> Artist(SiteId siteId, int artistId);
		Task<bool> ClearRating(SiteId siteId, int songId);
		Task<bool> ClearRequests();
		Task<List<User>> CurrentListeners(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<bool> DeleteRequest(int songId, SiteId siteId);
		Task<bool> FaveAlbum(SiteId siteId, int albumId, bool fave);
		Task<bool> FaveSong(SiteId siteId, int songId, bool fave);
		Task<Group> Group(SiteId siteId, int groupId);
		Task<Info> Info(SiteId siteId, bool currentListeners = false, bool allAlbums = false);
		Task<List<Info>> InfoAll(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<User> Listener(int id);
		Task<bool> OrderRequests(string songIdList, SiteId siteId);
		Task<bool> PauseRequestQueue(SiteId siteId);
		Task<List<Song>> PlaybackHistory(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<bool> Rate(SiteId siteId, int songId, decimal rating);
		Task<bool> Request(SiteId siteId, int songId);
		Task<bool> RequestFavoritedSongs(SiteId siteId, int limit = int.MaxValue);
		Task<List<Request>> RequestLine(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<bool> RequestUnratedSongs(SiteId siteId, int limit = 0);
		Task<Search> Search(SiteId siteId, string searchString);
		Task<Song> Song(SiteId siteId, int songId);
		Task<Dictionary<SiteId, int>> StationSongCount();
		Task<List<Station>> Stations(int perPage = 0, int pageStart = 0);
		Task<Info> Sync(SiteId siteId, bool offlineAck = false, bool resync = false, int knownEventId = -1);
		Task<List<Donation>> TipJar(int perPage = 0, int pageStart = 0);
		Task<List<Song>> Top100(int perPage = 0, int pageStart = 0);
		Task<List<Song>> UnratedSongs(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<bool> UnpauseRequestQueue(SiteId siteId);
		Task<User> UserInfo(int perPage = 0, int pageStart = 0);
		Task<List<Song>> UserRecentVotes(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<List<Song>> UserRequestedHistory(SiteId siteId, int perPage = 0, int pageStart = 0);
		Task<List<User>> UserSearch(string userName);
		Task<bool> Vote(SiteId siteId, int entryId);
	}
}
