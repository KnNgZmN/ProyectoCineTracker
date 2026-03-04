namespace CineTracker.Models
{
    public class WatchListItem
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PosterPath { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public double VoteAverage { get; set; }
        public bool IsFavorite { get; set; } = false;
        public bool IsWatched { get; set; } = false;
        public DateTime DateAdded { get; set; } = DateTime.Now;

    }
}
