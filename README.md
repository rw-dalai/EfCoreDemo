# EF Core Demo — Cheat Sheet

## EF Core Conventions

- **Table Name**: EF Core uses `DbSet<User> Users` -> table named `Users`
- **Primary Key**: EF Core uses property named `Id` or `<Type>Id` as PK
- **Foreign Key**: EF Core creates Shadow FK (DB column without C# property)
  - Name derived from navigation property (`User` -> `UserId`) or from type `<Type>Id`
- **Relationships**:
  - **1:1**: Fluent API needed, otherwise EF Core configures 1:n by convention
  - **1:n**: Fluent API not needed, EF Core configures 1:n by convention

## 1:1 Unidirektional (User -> Profile)

```csharp
modelBuilder.Entity<User>()
    .HasOne(u => u.Profile)
    .WithOne()
    .HasForeignKey<Profile>("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## 1:1 Bidirektional (User <-> Passport)

```csharp
modelBuilder.Entity<User>()
    .HasOne(u => u.Passport)
    .WithOne(p => p.User)
    .HasForeignKey<Passport>("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## 1:n Unidirektional (Order -> User)

```csharp
// EF Core auto-configures 1:n, but we can still manually do it
modelBuilder.Entity<Order>()
    .HasOne(o => o.User)
    .WithMany()
    .HasForeignKey("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## 1:n Bidirektional (User <-> Orders)

```csharp
// EF Core auto-configures 1:n, but we can still manually do it
modelBuilder.Entity<User>()
    .HasMany(u => u.Orders)
    .WithOne(o => o.User)
    .HasForeignKey("UserId")
    .OnDelete(DeleteBehavior.Cascade);
```

## ValueObjects, RichTypes, Enums

```csharp

// --- Configs ---

var order = modelBuilder.Entity<Order>();

// Enum -> string
order.Property(o => o.Status).HasConversion<string>();

// Enum -> custom value
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

// Normal Lookup: OrdersStatus -> string
Dictionary<OrderStatus, string> StatusToDb = new()
{
    [OrderStatus.Created] = "C",
    [OrderStatus.Paid] = "P"
};

// Inverse Lookup: string -> OrderStatus
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


## Configure Sqlite for Tests

```csharp
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
```
