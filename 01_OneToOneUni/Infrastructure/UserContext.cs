using Microsoft.EntityFrameworkCore;
using OneToOneUni.Models;

namespace OneToOneUni.Infrastructure;

/*
 * CREATE TABLE Users (
 *     Id       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     FullName TEXT NOT NULL
 *  );
 *
 * CREATE TABLE Profiles (
 *     Id     INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     Bio    TEXT    NOT NULL,
 *     UserId INTEGER NOT NULL,
 *     FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
 * );
 *
 * -> EF creates this automatically for 1:1 (prevents 1:1 from becoming 1:n)
 * CREATE UNIQUE INDEX IX_Profiles_UserId ON Profiles (UserId);
 */

public class UserContext(DbContextOptions opt) : DbContext(opt)
{
    // --- Database Tables ---
    
    // DbSet<T> properties represent tables in the database.
    
    public DbSet<User> Users => Set<User>();

    public DbSet<Profile> Profiles => Set<Profile>();
    
    
    // --- Fluent Configuration ---
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PATTERN: Unidirectional 1:1, User is principal, Profile is dependent (carries FK).
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)                 // 1 User -> 1 Profile
            .WithOne()                              // No navigation on Profile (unidirectional), EF creates unique index
            .HasForeignKey<Profile>("UserId")       // Profile carries FK
            .OnDelete(DeleteBehavior.Cascade);      // Deleting User deletes Profile.
    }
}
