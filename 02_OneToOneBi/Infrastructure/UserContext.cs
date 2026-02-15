using Microsoft.EntityFrameworkCore;
using OneToOneBi.Models;

namespace OneToOneBi.Infrastructure;

/*
 * CREATE TABLE Users (
 *     Id       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     FullName TEXT    NOT NULL
 * );
 *
 * CREATE TABLE Passports (
 *     Id     INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     Number TEXT    NOT NULL,
 *     UserId INTEGER NOT NULL,
 *     FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
 * );
 *
 * -> EF creates this automatically for 1:1 (prevents 1:1 from becoming 1:n)
 * CREATE UNIQUE INDEX IX_Passports_UserId ON Passports (UserId);
 */

public class UserContext(DbContextOptions opt) : DbContext(opt)
{
    // --- Database Tables ---

    // DbSet<T> properties represent tables in the database.

    public DbSet<User> Users => Set<User>();

    public DbSet<Passport> Passports => Set<Passport>();


    // --- Fluent Configuration ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PATTERN: Bidirectional 1:1, User is principal, Passport is dependent (carries FK).
        modelBuilder.Entity<User>()
            .HasOne(u => u.Passport)                // 1 User -> 1 Passport
            .WithOne(p => p.User)                   // 1 Passport -> 1 User (bidirectional), EF creates unique index
            .HasForeignKey<Passport>("UserId")      // Passport carries FK
            .OnDelete(DeleteBehavior.Cascade);      // Deleting User deletes Passport.
    }
}
