
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaSystem.DataAccess.EntityConfigurations
{
    public class ActorCategoryEntityTypeConfiguration : IEntityTypeConfiguration<ActorCategory>
    {
        public void Configure(EntityTypeBuilder<ActorCategory> builder)
        {
            builder.HasKey(e => new { e.ActorId, e.CategoryId });
        }
    }
}
