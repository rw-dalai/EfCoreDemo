using Microsoft.EntityFrameworkCore;
using Conversions.Models;

namespace Conversions.Infrastructure;

/*
 *   CREATE TABLE Orders (
 *       Id                       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *       Status                   TEXT    NOT NULL,
 *       CustomerEmail            TEXT    NOT NULL,
 *       ShippingAddress_Street   TEXT    NOT NULL,
 *       ShippingAddress_City     TEXT    NOT NULL,
 *       ShippingAddress_Zip      TEXT    NOT NULL
 *   );
 */

public class OrderContext(DbContextOptions opt) : DbContext(opt)
{
    // --- Database Tables ---

    public DbSet<Order> Orders => Set<Order>();
    

    // --- Fluent Configuration ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<Order>();

        // PATTERN 1: Enum -> string
        // order.Property(o => o.Status).HasConversion<string>();

        // PATTERN 2: Enum -> custom value
        order.Property(o => o.Status)
            .HasConversion(
                status => StatusToDb[status], // C# -> DB
                value => DbToStatus[value]);  // DB -> C#

        // PATTERN 3: Rich Type
        order.Property(o => o.CustomerEmail)
            .HasConversion(
                email => email.Value,       // C# -> DB
                value => new Email(value)); // DB -> C#

        // PATTERN 4: Value Object
        order.OwnsOne(o => o.ShippingAddress);
    }
    
    
    // --- Mapping Tables ---
    
    // Normal Lookup: OrdersStatus -> string
    private static readonly Dictionary<OrderStatus, string> StatusToDb = new()
    {
        [OrderStatus.Created] = "C",
        [OrderStatus.Paid] = "P"
    };

    // Inverse Lookup: string -> OrderStatus
    private static readonly Dictionary<string, OrderStatus> DbToStatus =
        StatusToDb.ToDictionary(x => x.Value, x => x.Key);
}
