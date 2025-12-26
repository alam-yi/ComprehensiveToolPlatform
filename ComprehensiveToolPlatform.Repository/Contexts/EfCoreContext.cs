using System;
using System.Collections.Generic;
using ComprehensiveToolPlatform.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace ComprehensiveToolPlatform.Repository.Contexts;

public partial class EfCoreContext : DbContext
{
    public EfCoreContext()
    {
    }

    public EfCoreContext(DbContextOptions<EfCoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("Application");

            entity.Property(e => e.Id).HasMaxLength(40);
            entity.Property(e => e.CategoryId).HasMaxLength(40);
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.FileType).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.Id).HasMaxLength(40);
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
