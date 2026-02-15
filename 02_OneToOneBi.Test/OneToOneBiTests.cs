using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneToOneBi.Infrastructure;
using OneToOneBi.Models;

namespace OneToOneBi.Test;

public class OneToOneBiTests
{
    private UserContext GetDatabase()
    {
        // Create in-memory SQLite database
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var opt = new DbContextOptionsBuilder<UserContext>()
            .UseSqlite(connection)
            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new UserContext(opt);
        Debug.WriteLine(db.Database.GenerateCreateScript());
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public void CreateDatabaseSuccessTest()
    {
        using var db = GetDatabase();
        Assert.True(db.Database.CanConnect());
    }
    
    [Fact]
    public void SaveAndRetrieveTest()
    {
        using var db = GetDatabase();

        // GIVEN : User with a Passport (1:1 bidirectional)
        var passport = new Passport("AT-1234567");
        var user = new User("Ada Lovelace", passport);

        // WHEN : Insert User into DB (also inserts Passport)
        db.Users.Add(user);
        db.SaveChanges();
        db.ChangeTracker.Clear(); // Clear cache

        // THEN : User -> Passport works
        //  SELECT *
        //  FROM Users u
        //  LEFT JOIN Passports p ON u.Id = p.UserId
        //  WHERE u.Id = @id
        var retrievedUser = db.Users
            // Include: Load Passport when loading User (-> Eager loading)
            .Include(u => u.Passport)
            .Single(u => u.Id == user.Id);
        Assert.Equal(passport.Number, retrievedUser.Passport.Number);

        // THEN : Passport -> User works (bidirectional)
        //  SELECT *
        //  FROM Passports p
        //  LEFT JOIN Users u ON p.UserId = u.Id
        //  WHERE p.Id = @id
        var retrievedPassport = db.Passports
            // Include: Load User when loading Passport (-> Eager loading)
            .Include(p => p.User)
            .Single(p => p.Id == passport.Id);
        Assert.Equal(user.FullName, retrievedPassport.User.FullName);
    }
}
