using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Models
{
    
    public class Actor
    {
      public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public string Img { get; set; } = "defaultImg.png"; 
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        //public int ActorId { get; set; }
        //public Actor? actorId { get; set; }
    }
}
