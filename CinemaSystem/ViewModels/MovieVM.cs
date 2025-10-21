namespace CinemaSystem.ViewModels
{
    public class MovieVM
    {
        public IEnumerable<Category> Categories { get; set; } = null!;
        public IEnumerable<Cinema> Cinemas { get; set; } = null!;
        public IEnumerable<Actor> actors { get; set; } = null!;
        public IEnumerable<MovieSubImage> MovieSubImages { get; set; } = null!;
        public Movie? Movie { get; set; }
    }
}
