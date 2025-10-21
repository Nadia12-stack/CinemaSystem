namespace CinemaSystem.Models
{
    public class ActorCategory
    {
        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string MainRole { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = "Intermediate"; 
        public DateTime StartDate { get; set; } = DateTime.Now;
    }
}
