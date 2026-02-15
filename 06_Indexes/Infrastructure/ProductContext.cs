using Microsoft.EntityFrameworkCore;
using Indexes.Models;

namespace Indexes.Infrastructure;

/*
    CREATE TABLE Products (
        ArticleNumber TEXT NOT NULL PRIMARY KEY,
        Name          TEXT NOT NULL,
        Category      TEXT NOT NULL,
        Price         TEXT NOT NULL
    );

    CREATE INDEX IX_Products_Category ON Products (Category);
    CREATE UNIQUE INDEX IX_Products_Category_Name ON Products (Category, Name);
*/

public class ProductContext(DbContextOptions opt) : DbContext(opt)
{
    // --- Database Tables ---

    public DbSet<Product> Products => Set<Product>();


    // --- Fluent Configuration ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();

        // PATTERN 1: Natural key as Primary Key
        product.HasKey(p => p.ArticleNumber);
        
        // PATTERN 2: Single-field NON-UNIQUE index
        product.HasIndex(p => p.Category); /*.IsUnique()*/

        // PATTERN 3: Composite UNIQUE index
        product.HasIndex(p => new { p.Category, p.Name }).IsUnique();
    }
}
