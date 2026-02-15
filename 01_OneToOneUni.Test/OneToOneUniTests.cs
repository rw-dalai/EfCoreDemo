using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneToOneUni.Infrastructure;
using OneToOneUni.Models;

namespace OneToOneUni.Test;

public class OneToOneUniTests
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

        // GIVEN : User with a Profile (1:1 unidirectional)
        var profile = new Profile("Programmer");
        var user = new User("Ada Lovelace", profile);

        // WHEN : Insert User into DB (also inserts Profile)
        db.Users.Add(user);
        db.SaveChanges();
        db.ChangeTracker.Clear(); // Clear cache

        // THEN : Loading User with Profile works
        //  SELECT *
        //  FROM Users u
        //  LEFT JOIN Profiles p ON u.Id = p.UserId
        //  WHERE u.Id = @id
        var retrievedUser = db.Users
            // Include: Load Profile when loading User (-> Eager loading)
            .Include(u => u.Profile)
            .Single(u => u.Id == user.Id);
        Assert.Equal(profile.Bio, retrievedUser.Profile.Bio);
    }

    
    // BONUS : LAZY LOADING
    [Fact]
    public void LazyLoadingTest()
    {
        using var db = GetDatabase();

        // GIVEN : User with a Profile (1:1 unidirectional)
        var profile = new Profile("Programmer");
        var user = new User("Ada Lovelace", profile);

        // WHEN : Insert User into DB (also inserts Profile)
        db.Users.Add(user);
        db.SaveChanges();
        db.ChangeTracker.Clear(); // Clear cache

        // THEN : Loading User WITHOUT Include (Profile is null)
        //  SELECT * FROM Users WHERE Id = @id
        var retrievedUser = db.Users.Single(u => u.Id == user.Id);
        Assert.Null(retrievedUser.Profile); // not loaded yet!

        // THEN : Explicit loading — load Profile separately
        //  SELECT * FROM Profiles WHERE UserId = @id
        db.Entry(retrievedUser).Reference(u => u.Profile).Load();
        Assert.Equal(profile.Bio, retrievedUser.Profile.Bio); // now loaded!
    }
}
