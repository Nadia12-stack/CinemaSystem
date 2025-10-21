using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Models
{

    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Awards { get; set; }
        public bool Status { get; set; }
        public string Img { get; set; } = "defaultImg.png";
        public DateTime BirthDate { get; set; }
        public string Nationality { get; set; } = string.Empty;

        public List<MovieActor> MovieActors { get; set; } = new();
        public List<ActorCategory> ActorCategories { get; set; } = new();

        public List<SocialLink> SocialLinks { get; set; } = new();

    }
}
