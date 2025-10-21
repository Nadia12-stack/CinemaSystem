using CinemaSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.DataAccess.EntityConfigurations
{
    public class ActorImgEntityTypeConfiguration : IEntityTypeConfiguration<MovieSubImage>
    {
        public void Configure(EntityTypeBuilder<MovieSubImage> builder)
        {
            builder.HasKey(e => new { e.MovietId, e.Img });
        }
    }
}
