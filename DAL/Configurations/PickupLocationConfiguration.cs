using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class PickupLocationConfiguration : IEntityTypeConfiguration<PickupLocation>
{
    public void Configure(EntityTypeBuilder<PickupLocation> builder)
    {
        builder.ToTable("PickupLocations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AreaName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Instructions)
            .HasMaxLength(1000);

        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Farmer)
            .WithMany(x => x.PickupLocations)
            .HasForeignKey(x => x.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany(x => x.PickupLocations)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CityId, x.IsActive });
        builder.HasIndex(x => new { x.FarmerId, x.IsActive });
    }
}