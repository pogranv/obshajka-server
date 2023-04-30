using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Obshajka.DbManager.Postgres.Models;

using Microsoft.Extensions.Configuration;
using System.Data;

namespace Obshajka.DbManager.Postgres;

public partial class ObshajkaDbContext : DbContext
{

    private static readonly string s_connectionString;

    static ObshajkaDbContext() 
    {
        s_connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
            .GetSection("DbConnectionStrings")["DefaultConnection"];
    }
    public ObshajkaDbContext()
    {
    }

    public ObshajkaDbContext(DbContextOptions<ObshajkaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Advertisement> Advertisements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(s_connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("advertisements_pkey");

            entity.ToTable("advertisements");

            entity.HasIndex(e => new { e.CreatorId, e.DormitoryId }, "dormitory_id_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.DateOfAddition).HasColumnName("date_of_addition");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DormitoryId).HasColumnName("dormitory_id");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Title)
                .HasMaxLength(30)
                .HasColumnName("title");

            entity.HasOne(d => d.Creator).WithMany(p => p.Advertisements)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("advertisements_creator_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(35)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(35)
                .HasColumnName("password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
