using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskProcessingAPI.Domain.Entities;

namespace TaskProcessingAPI.Infrastructure.Persistence.EntityConfiguration
{
    public class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
    {
        public void Configure(EntityTypeBuilder<TaskEntity> builder)
        {
            builder.Property(x => x.UserId)
                 .IsRequired(false);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.UserId);
        }
    }
}
