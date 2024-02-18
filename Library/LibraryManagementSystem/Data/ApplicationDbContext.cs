using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookReservation> BookReservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                entity.Property(u => u.IsAdmin)
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();

            modelBuilder.Entity<BookReservation>()
                .HasOne(br => br.Book)
                .WithMany()
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookReservation>()
                .HasOne(br => br.User)
                .WithMany()
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.User)
                .WithMany(u => u.Borrowings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.Book)
                .WithMany(b => b.Borrowings)
                .HasForeignKey(b => b.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Tags)
                .WithMany(t => t.Books);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
} 