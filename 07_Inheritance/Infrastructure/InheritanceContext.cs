using Microsoft.EntityFrameworkCore;
using Inheritance.Models;

namespace Inheritance.Infrastructure;

/*
    CREATE TABLE PaymentMethods (
        Id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
        Owner       TEXT    NOT NULL,
        PaymentType TEXT    NOT NULL,
        CardNumber  TEXT    NULL,
        ExpiryDate  TEXT    NULL,
        Email       TEXT    NULL
    );
*/

public class InheritanceContext(DbContextOptions opt) : DbContext(opt)
{
    // --- Database Tables ---

    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    
    // --- Fluent Configuration ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PATTERN: All subclasses in ONE table (TPH, Table per Inheritance), distinguished by a discriminator column
        modelBuilder.Entity<PaymentMethod>()
            .HasDiscriminator<string>("PaymentType") // Discriminator column, holds the subclass type as string
            .HasValue<CreditCard>("CreditCard")      // CreditCard rows have "CreditCard" in PaymentType
            .HasValue<PayPal>("PayPal");             // PayPal rows have "PayPal" in PaymentType
    }
}
