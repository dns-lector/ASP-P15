﻿using Microsoft.EntityFrameworkCore;

namespace ASP_P15.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entities.User>  Users  { get; set; }
        public DbSet<Entities.Token> Tokens { get; set; }
        public DbSet<Entities.Product> Products { get; set; }
        public DbSet<Entities.ProductGroup> Groups { get; set; }
        public DbSet<Entities.Feedback> Feedbacks { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Feedback>()
                .HasOne(f => f.Product)
                .WithMany(p => p.Feedbacks);
            modelBuilder.Entity<Entities.Feedback>()
                .HasOne(f => f.User)
                .WithMany(u => u.Feedbacks);

            modelBuilder.Entity<Entities.User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Entities.Product>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Products)
                .HasForeignKey(p => p.GroupId)
                .HasPrincipalKey(g => g.Id);
            modelBuilder.Entity<Entities.Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<Entities.ProductGroup>()
                .HasIndex(g => g.Slug)
                .IsUnique();

        }
    }
}
