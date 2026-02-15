using Microsoft.EntityFrameworkCore;
using OneToManyBi.Models;

namespace OneToManyBi.Infrastructure;

/*
 * CREATE TABLE Users (
 *     Id       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     FullName TEXT    NOT NULL
 * );
 *
 * CREATE TABLE Orders (
 *     Id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     OrderNumber TEXT    NOT NULL,
 *     UserId      INTEGER NOT NULL,
 *     FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
 * );
 *
 * CREATE INDEX IX_Orders_UserId ON Orders (UserId);
 */

public class UserContext(DbContextOptions opt) : DbContext(opt)
{
    // --- Database Tables ---

    // DbSet<T> properties represent tables in the database.

    public DbSet<User> Users => Set<User>();

    public DbSet<Order> Orders => Set<Order>();


    // --- Fluent Configuration ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PATTERN: 1:n bidirectional, User is the principal, Order is the dependent (carries FK)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Orders)              // 1 User -> n Orders
            .WithOne(o => o.User)                // 1 Order -> 1 User (bidirectional)
            // .HasForeignKey("UserId")          // Optional for 1:n (EF knows), but names the shadow FK explicitly
            .OnDelete(DeleteBehavior.Cascade);   // Deleting User deletes Orders.
    }
}
