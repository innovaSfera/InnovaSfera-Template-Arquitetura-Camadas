using DomainDrivenDesign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivenDesign.Infrastructure.Data.Configurations;

public class SampleDataConfiguration : IEntityTypeConfiguration<SampleData>
{
    public void Configure(EntityTypeBuilder<SampleData> builder)
    {
        builder.ToTable("SampleData");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Message).IsRequired().HasMaxLength(250);

        builder.Property(p => p.TimeStamp).HasDefaultValue(DateTime.UtcNow);
    }
}
