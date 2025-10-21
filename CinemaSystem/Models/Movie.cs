namespace CinemaSystem.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }//On Ticket
        public double Rate { get; set; }
        public string MainImg { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public List<MovieActor> MovieActors { get; set; } = new();

        
        public List<MovieSubImage> movieSubImages { get; set; } = null!;
        public List<MovieCinema> MovieCinemas { get; set; } = new();

    }
}
