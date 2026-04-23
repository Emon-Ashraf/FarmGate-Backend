using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.UserId);

        builder.HasOne(x => x.PrimaryCity)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.PrimaryCityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}