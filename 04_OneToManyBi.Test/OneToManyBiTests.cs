using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneToManyBi.Infrastructure;
using OneToManyBi.Models;

namespace OneToManyBi.Test;

public class OneToManyBiTests
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

        // GIVEN : User with two Orders (1:n bidirectional)
        var user = new User("Ada Lovelace");
        var order1 = new Order("Order-1");
        var order2 = new Order("Order-2");
        user.AddOrder(order1);
        user.AddOrder(order2);

        // WHEN : Insert User (cascade inserts all Orders in the collection)
        db.Users.Add(user);
        db.SaveChanges();
        db.ChangeTracker.Clear(); // Clear cache

        // THEN : User -> Orders works
        //  SELECT *
        //  FROM Users u
        //  LEFT JOIN Orders o ON u.Id = o.UserId
        //  WHERE u.Id = @id
        var retrievedUser = db.Users
            // Include: Load Orders when loading User (-> Eager loading)
            .Include(u => u.Orders)
            .Single(u => u.Id == user.Id);
        Assert.Equal(2, retrievedUser.Orders.Count);

        // THEN : Order -> User works (bidirectional)
        //  SELECT *
        //  FROM Orders o
        //  LEFT JOIN Users u ON o.UserId = u.Id
        //  WHERE o.Id = @id
        var retrievedOrder = db.Orders
            // Include: Load User when loading Order (-> Eager loading)
            .Include(o => o.User)
            .Single(o => o.Id == order1.Id);
        Assert.Equal(user.FullName, retrievedOrder.User.FullName);
    }
}
