using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ModifayIdentity(modelBuilder);
            ModifayCategory(modelBuilder);
            ModifayAuthor(modelBuilder);
            ModifayBook(modelBuilder);
            ModifayBookCopy(modelBuilder);

            modelBuilder.Entity<BookCategory>()
                .ToTable(name: "BookCategories", schema: "Library")
                .HasKey(k => new { k.BookId, k.CategoryId }).IsClustered();
        }
        private void ModifayIdentity(ModelBuilder modelBuilder)
        {
            var user = modelBuilder.Entity<ApplicationUser>().ToTable("Users", "Auth");
            user.Property(p => p.CreateOn).HasDefaultValueSql("GETDATE()");
            user.HasIndex(p => p.Email).IsUnique().HasFilter(null);
            user.HasIndex(p => p.UserName).IsUnique().HasFilter(null);
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "Auth");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "Auth");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "Auth");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "Auth");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "Auth");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "Auth");
        }
        private void ModifayCategory(ModelBuilder modelBuilder)
        {
            var category = modelBuilder.Entity<Category>().ToTable("Categories", "Library");
            category.HasKey(p => p.Id);
            category.HasIndex(p => p.Name).IsUnique();
            category.Property(p => p.CreateOn).HasDefaultValueSql("GETDATE()");
            category.Property(p => p.Name).HasMaxLength(100);
        }
        private void ModifayAuthor(ModelBuilder modelBuilder)
        {
            var Author = modelBuilder.Entity<Author>().ToTable("Authors", "Library");
            Author.HasKey(a => a.Id);
            Author.Property(a => a.Name).HasMaxLength(100);
            Author.Property(a => a.CreateOn).HasDefaultValueSql("GETDATE()");
            Author.HasIndex(a => a.Name).IsUnique();
        }
        private void ModifayBook(ModelBuilder modelBuilder)
        {
            var Book = modelBuilder.Entity<Book>().ToTable(name: "Books", schema: "Library");
            Book.Property(b => b.CreateOn).HasDefaultValueSql("GETDATE()");
            Book.HasIndex(b => new { b.Title, b.AuthorId }).IsUnique();
        }
        private void ModifayBookCopy(ModelBuilder modelBuilder)
        {
            var Copy = modelBuilder.Entity<BookCopy>().ToTable(name: "BookCopies", schema: "Library");
            Copy.Property(b => b.CreateOn).HasDefaultValueSql("GETDATE()");

            modelBuilder.HasSequence("SerialNumber", schema: "shared")
                .StartsAt(1000001)
                .IncrementsBy(10);

            Copy.Property(c => c.SerialNumber).HasDefaultValueSql("Next Value For shared.SerialNumber");
        }
    }
}
