using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Conversions.Infrastructure;
using Conversions.Models;

namespace Conversions.Test;

public class ConversionsTests
{
    private OrderContext GetDatabase()
    {
        // Create in-memory SQLite database
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var opt = new DbContextOptionsBuilder<OrderContext>()
            .UseSqlite(connection)
            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new OrderContext(opt);
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

        // GIVEN : an Order with enum status, value object email and owned address
        var order = new Order(
            OrderStatus.Paid,
            new Email("ada@spengergasse.at"),
            new Address("Spengergasse 20", "Wien", "1050"));

        // WHEN : Order is inserted into DB
        db.Orders.Add(order);
        db.SaveChanges();
        db.ChangeTracker.Clear(); // Clear cache

        // THEN : Loading Order works
        //  SELECT *
        //  FROM Orders
        //  WHERE Id = @id
        var retrieved = db.Orders.Single(o => o.Id == order.Id);
        Assert.Equal(OrderStatus.Paid, retrieved.Status);
        Assert.Equal(new Email("ada@spengergasse.at"), retrieved.CustomerEmail);
        Assert.Equal(new Address("Spengergasse 20", "Wien", "1050"), retrieved.ShippingAddress);
    }
}
