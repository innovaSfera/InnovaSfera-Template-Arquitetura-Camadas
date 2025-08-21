using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenDesign.Infrastructure.Data.Context;

public class SampleContext : DbContext
{
    public SampleContext(DbContextOptions options)
          : base(options)
    {
    }

    public DbSet<SampleData> SampleDatas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SampleDataConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
