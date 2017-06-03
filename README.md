# Rainwave Client

## What is Rainwave?
[Rainwave](http://www.rainwave.cc) is a free online music streaming service, focused around video game-related music. Some of the streams (called stations) include original video game music, remixed music (courtesy of [Overclocked Remix](http://www.ocremix.org)), and chiptunes.

Songs that are played are based on a popular election of the listeners - the song with the most votes is played. Listeners can also request specific songs, which are intermittently added as an election options.

Additionally, Rainwave offers a rating system for songs, which allow listeners to rate song on a 1-5 scale and mark songs and albums as 'favorite' through a modern web interface.

## What is the Rainwave Client?
The Rainwave Client is a .NET-based windows application with a primary focus to help users listen to new or unrated music on Rainwave while not having to actively switch to the web page every 2-5 minutes when a song changes. Note that currently there is not a mechanism to help with rating songs through this client, and must be done through the web.

The client can be configured to determine what songs the user should vote for, and will automatically vote in each election on the user's behalf. 

The client also automatically ensures the request queue for the user is always populated with eligible songs. If the number of songs in the request queue falls below a user-defined limit, unrated songs will be automatically added to the queue. Songs in the request queue which are no longer eligible to be played (normally due to cooldown) are automatically removed. 

Finally, if the user stops listening, the request queue and voting is automatically paused. The client can also be configured to automatically pause or unpause the request queue and voting at specific times of the day.


## Running client
To run the client, you must first provide it with a User ID and API key. You can get this information and generate new API keys after you are logged into Rainwave by going to [http://rainwave.cc/keys/](http://rainwave.cc/keys/). Your user ID will be listed at the top, and current API keys listed (with the most recent at the top). This information should be added to the relevant settings (`UserId` and `ApiKey`, respectively) in the `App.config`.

When the client is started, it attempts to automatically determine the station (if any) the user is listening to, and proceeds to vote on any eligible elections for that station. If the user switches stations, the client automatically picks up the change after the current song on the old station is completed.


### Automatic Voting
Voting preferences can be adjusted in the `VotingPreferences.xml` file. The file has the following format:

```xml
<VotingPreferences>
  <VotePreference>
    <SortOrder>[Decimal]</SortOrder>
    <IsRequest>[Boolean]</IsRequest>
    <IsMyRequest>[Boolean]</IsMyRequest>
    <IsFavorite>[Boolean]</IsFavorite>
    <SongRating>[Decimal]</SongRating>
  </VotePreference>
  <VotePreference>
  ...
  </VotePreference>
</VotingPreference>
```

All nodes except `SortOrder` are optional, so can be mixed and matched as desired.

For every eligible election that you have not already voted in, the client goes through each song in the election and matches it with the first `VotePreference` entry it can find. The songs are then ordered by:
- The `SortOrder` of the found VotePreference (ascending) (if one was not found, a higher value is used)
- The current song rating (descending) 
- The current album rating (descending)

The song at the top of this list gets the final vote.

Note that voting will only occur if the user's request queue is unpaused.