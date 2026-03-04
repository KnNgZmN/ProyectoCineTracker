namespace CineTracker.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string PosterPath { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public double VoteAverage { get; set; }
        public string Genres { get; set; } = string.Empty;
        
    }
}
