using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Obshajka.DbManager.Postgres.Models;

namespace Obshajka.DbManager.Postgres;

public partial class ObshajkaDbContext : DbContext
{
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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=obshajka_db;Username=postgres;Password=andrew7322");

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
