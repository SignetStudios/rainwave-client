namespace SS.RainwaveClient
{
	public class VotePriority
	{
		public VotePriority() : this (false, false, null, false, 0)
		{
			
		}

		public VotePriority(bool? isRequest,
			bool? isMyRequest,
			double? songRating,
			bool? isFavorite,
			int sortOrder)
		{
			IsMyRequest = isMyRequest;
			IsRequest = isRequest;
			SongRating = (decimal?) songRating;
			IsFavorite = isFavorite;
			SortOrder = sortOrder;
		}

		public VotePriority(bool? isRequest,
			bool? isMyRequest,
			int? songRating,
			bool? isFavorite,
			int sortOrder) : this (isRequest, isMyRequest, (double?) songRating, isFavorite, sortOrder)
		{
		}

		public bool? IsRequest { get; set; }
		public bool? IsMyRequest { get; set; }
		public decimal? SongRating { get; set; }
		public bool? IsFavorite { get; set; }
		public int SortOrder { get; set; }
	}


}
