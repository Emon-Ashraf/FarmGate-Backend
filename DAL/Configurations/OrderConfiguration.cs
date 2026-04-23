using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EstimatedProductTotal)
            .HasPrecision(18, 2);

        builder.Property(x => x.ActualProductTotal)
            .HasPrecision(18, 2);

        builder.Property(x => x.ServiceFeeAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PickupCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.PickupCitySnapshot)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.PickupAreaSnapshot)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.PickupAddressSnapshot)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.PickupInstructionsSnapshot)
            .HasMaxLength(1000);

        builder.Property(x => x.FarmerPhoneSnapshot)
            .HasMaxLength(30);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Farmer)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PickupLocation)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.PickupLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CustomerId, x.Status });
        builder.HasIndex(x => new { x.FarmerId, x.Status });
        builder.HasIndex(x => x.PickupLocationId);
    }
}