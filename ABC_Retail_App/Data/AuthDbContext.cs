using ABC_Retail_App.Models;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace ABC_Retail_App.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        // DbSets for tables
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Cart { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity to match existing table structure
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Username).HasColumnName("Username").IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash").IsRequired().HasMaxLength(256);
                entity.Property(e => e.Role).HasColumnName("Role").IsRequired().HasMaxLength(20).HasDefaultValue("Customer");

                // Create unique index on Username
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configure Cart entity to match existing table structure
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Cart");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.Property(e => e.CustomerUsername).HasColumnName("CustomerUsername").IsRequired().HasMaxLength(100);
                entity.Property(e => e.ProductId).HasColumnName("ProductId").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Quantity).HasColumnName("Quantity").IsRequired();

                // Create index on CustomerUsername for faster queries
                entity.HasIndex(e => e.CustomerUsername);
            });

            // Seed data to match your existing data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "customer101",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("customerpass123"),
                    Role = "Customer"
                },
                new User
                {
                    Id = 2,
                    Username = "admin01",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpass123"),
                    Role = "Admin"
                }
            );
        }
    }
}