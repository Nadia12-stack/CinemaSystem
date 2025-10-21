namespace CinemaSystem.ViewModels
{
    public class MovieVM
    {
        public IEnumerable<Category> Categories { get; set; } = null!;
        public IEnumerable<Cinema> Cinemas { get; set; } = null!;
        public IEnumerable<Actor> actors { get; set; } = null!;
        public IEnumerable<MovieCinema> MovieCinemas { get; set; } = null!;
        public IEnumerable<SocialLink> socialLinks { get; set; } = null!;
        public IEnumerable<MovieActor> MovieActors { get; set; } = null!;
        public IEnumerable<MovieCinema> movieCinemas { get; set; } = null!;
        public IEnumerable<ActorCategory> actorCategories { get; set; } = null!;
        public IEnumerable<MovieSubImage> MovieSubImages { get; set; } = null!;
        public Movie? Movie { get; set; }
    }
}
