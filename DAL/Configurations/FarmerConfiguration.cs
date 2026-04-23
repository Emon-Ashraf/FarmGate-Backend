using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class FarmerConfiguration : IEntityTypeConfiguration<Farmer>
{
    public void Configure(EntityTypeBuilder<Farmer> builder)
    {
        builder.ToTable("Farmers");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsProfileCompleted)
            .HasDefaultValue(false);

        builder.HasOne(x => x.PrimaryCity)
            .WithMany(x => x.Farmers)
            .HasForeignKey(x => x.PrimaryCityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}