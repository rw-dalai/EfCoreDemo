// PRIMARY KEY
// EF convention: a property called "Id" or "<Type>Id" is the PK.
// If the PK has a different name we must configure it via HasKey in FluentAPI.

// NATURAL KEY vs SURROGATE KEY
// Surrogate key: artificial, no business meaning (auto-increment int).
// Natural key: real business value as PK (e.g. ArticleNumber).

// INDEXES
// Indexes speed up lookups; UNIQUE indexes also enforce constraints.

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

namespace Indexes.Models;

public class Product
{
    // --- Properties ---

    public string ArticleNumber { get; private set; } // Natural key (PK)

    public string Name { get; set; }

    public string Category { get; set; }

    public decimal Price { get; set; }


    // --- EF Ctor ---

    protected Product() { }


    // --- Business Ctor ---

    public Product(string articleNumber, string name, string category, decimal price)
    {
        ArticleNumber = articleNumber;
        Name = name;
        Category = category;
        Price = price;
    }
}
