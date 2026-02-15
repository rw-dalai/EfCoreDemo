using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Inheritance.Infrastructure;
using Inheritance.Models;

namespace Inheritance.Test;

public class InheritanceTests
{
    private InheritanceContext GetDatabase()
    {
        // Create in-memory SQLite database
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var opt = new DbContextOptionsBuilder<InheritanceContext>()
            .UseSqlite(connection)
            .LogTo(message => Debug.WriteLine(message), Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new InheritanceContext(opt);
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

        // GIVEN : Payments
        var card = new CreditCard("Ada Lovelace", "1111-1111-1111-1111", "12/2028");
        var paypal = new PayPal("Alan Turing", "alan@turing.com");

        // WHEN : Insert Payments into DB
        db.PaymentMethods.AddRange(card, paypal);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        // THEN : All payment methods live in one table
        Assert.Equal(2, db.PaymentMethods.Count());

        // THEN : OfType<T>() filters by discriminator
        var cards = db.PaymentMethods.OfType<CreditCard>().ToList();
        Assert.Single(cards);
        Assert.Equal("1111-1111-1111-1111", cards[0].CardNumber);

        var paypals = db.PaymentMethods.OfType<PayPal>().ToList();
        Assert.Single(paypals);
        Assert.Equal("alan@turing.com", paypals[0].Email);
    }
}
