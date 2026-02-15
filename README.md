# EF Core Demo — Cheat Sheet

## EF Core Conventions

- **Primary Key**: Property named `Id` or `<Type>Id` is automatically the PK
- **Foreign Key**: Navigation properties auto-generate a shadow FK (`UserId`)
- **Table Name**: `DbSet<User> Users` -> table named `Users`

## 1:1 Unidirektional (User -> Profile)

```csharp
// HasForeignKey<T> is required for 1:1 — EF can't determine which side carries the FK.
modelBuilder.Entity<User>()
    .HasOne(u => u.Profile)
    .WithOne()
    .HasForeignKey<Profile>("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## 1:1 Bidirektional (User <-> Passport)

```csharp
// HasForeignKey<T> is required for 1:1 — EF can't determine which side carries the FK.
modelBuilder.Entity<User>()
    .HasOne(u => u.Passport)
    .WithOne(p => p.User)
    .HasForeignKey<Passport>("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## 1:n Unidirektional (Order -> User)

```csharp
// HasForeignKey is optional for 1:n — EF knows the FK goes on the "many" side.
modelBuilder.Entity<Order>()
    .HasOne(o => o.User)
    .WithMany()
    //.HasForeignKey("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## 1:n Bidirektional (User <-> Orders)

```csharp
// HasForeignKey is optional for 1:n — EF knows the FK goes on the "many" side.
modelBuilder.Entity<User>()
    .HasMany(u => u.Orders)
    .WithOne(o => o.User)
    //.HasForeignKey("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## ValueObjects, RichTypes, Enums

```csharp

// --- Configs ---

// Enum -> string
order.Property(o => o.Status).HasConversion<string>();

order.Property(o => o.Status)
    .HasConversion(
        status => StatusToDb[status],
        value => DbToStatus[value]);

// Rich Type (1 Property -> 1 Column)
order.Property(o => o.CustomerEmail)
    .HasConversion(
        email => email.Value,
        value => new Email(value));

// Value Object (n Properties -> n Columns)
order.OwnsOne(o => o.ShippingAddress);

// --- Mappings ---

// Enum -> custom value (Dictionary mapping)
Dictionary<OrderStatus, string> StatusToDb = new()
{
    [OrderStatus.Created] = "C",
    [OrderStatus.Paid] = "P"
};
Dictionary<string, OrderStatus> DbToStatus =
    StatusToDb.ToDictionary(x => x.Value, x => x.Key);
```

## Indexes

```csharp
// Natural key als Primary Key
product.HasKey(p => p.ArticleNumber);

// NON-UNIQUE index
product.HasIndex(p => p.Category);

// Composite UNIQUE index
product.HasIndex(p => new { p.Category, p.Name }).IsUnique();
```

## Inheritance (TPH)

```csharp
modelBuilder.Entity<PaymentMethod>()
    .HasDiscriminator<string>("PaymentType")
    .HasValue<CreditCard>("CreditCard")
    .HasValue<PayPal>("PayPal");
```
