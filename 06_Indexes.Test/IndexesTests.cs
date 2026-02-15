using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Indexes.Infrastructure;
using Indexes.Models;

namespace Indexes.Test;

public class IndexesTests
{
    private ProductContext GetDatabase()
    {
        // Create in-memory SQLite database
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var opt = new DbContextOptionsBuilder<ProductContext>()
            .UseSqlite(connection)
            .LogTo(message => Debug.WriteLine(message), Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new ProductContext(opt);
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
    public void SaveAndRetrieveWithNaturalKeyTest()
    {
        using var db = GetDatabase();

        // GIVEN : Product with a ArticleNumber (natural key)
        var product = new Product("ART-1", "Laptop", "Electronics", 999.99m);

        // WHEN : Insert Product into DB
        db.Products.Add(product);
        db.SaveChanges();
        db.ChangeTracker.Clear();

        // THEN : Find by ArticleNumber (natural key) works
        //  SELECT * FROM Products WHERE ArticleNumber = @id
        var loaded = db.Products.Single(p => p.ArticleNumber == "ART-1");
        Assert.Equal("Laptop", loaded.Name);
        Assert.Equal(999.99m, loaded.Price);
    }

    [Fact]
    public void DuplicateArticleNumberTest()
    {
        using var db = GetDatabase();

        // GIVEN : Insert Product
        db.Products.Add(new Product("ART-1", "Laptop", "Electronics", 999));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        // WHEN : Insert Product (same ArticleNumber)
        db.Products.Add(new Product("ART-1", "Tablet", "Electronics", 499));

        // THEN : primary key violation
        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void DuplicateNameAndCategoryTest()
    {
        using var db = GetDatabase();

        // GIVEN : Insert Products (same Name and different Category)
        db.Products.Add(new Product("ART-01", "Laptop", "Electronics", 999));
        db.Products.Add(new Product("ART-02", "Laptop", "Software", 49));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        // WHEN : Insert Product (same Name and same Category)
        db.Products.Add(new Product("ART-012", "Laptop", "Electronics", 799));

        // THEN : composite unique constraint violation
        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }
}
