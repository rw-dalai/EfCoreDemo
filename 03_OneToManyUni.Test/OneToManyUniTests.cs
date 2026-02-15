using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneToManyUni.Infrastructure;
using OneToManyUni.Models;

namespace OneToManyUni.Test;

public class OneToManyUniTests
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

        // GIVEN : User with two Orders (1:n unidirectional)
        var user = new User("Ada Lovelace");
        var order1 = new Order("Order-1", user);
        var order2 = new Order("Order-2", user);

        // WHEN : Insert Orders into DB (also inserts User)
        db.Orders.AddRange(order1, order2);
        db.SaveChanges();
        db.ChangeTracker.Clear(); // Clear cache

        // THEN : Loading Order with User works (Order -> User)
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
