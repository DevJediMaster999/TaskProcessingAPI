using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskProcessingAPI.Domain.Entities;

namespace TaskProcessingAPI.Infrastructure.Persistence.EntityConfiguration
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasAlternateKey(x => x.Name);

            builder.HasMany(x => x.Tasks)
                 .WithOne(x => x.User)
                 .HasForeignKey(x => x.UserId);
        }
    }
}
