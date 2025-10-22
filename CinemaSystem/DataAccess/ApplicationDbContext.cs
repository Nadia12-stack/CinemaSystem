
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieActor> movieActors { get; set; }
        public DbSet<MovieCinema> movieCinemas { get; set; }
        public DbSet<SocialLink> socialLinks { get; set; }
        public DbSet<ActorCategory> actorCategories { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieSubImage> MovieSubImages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                    : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=CinemaSystem;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovieEntityTypeConfiguration).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
