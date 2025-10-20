using CinemaSystem.DataAccess.EntityConfigurations;
using CinemaSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace CinemaSystem.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Cinema> cinemas { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> movies { get; set; }
        public DbSet<Actor> actors { get; set; }
        public DbSet<MovieSubImage> movieSubImages { get; set; }

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
