# EF Core Configs

## EF Core Conventions

- **Table Name**: EF Core uses `DbSet<User> Users` -> table named `Users`
- **Primary Key**: EF Core uses property named `Id` or `<Type>Id` as PK
- **Foreign Key**: EF Core creates Shadow FK (DB column without C# property)
  - Name derived from navigation property (`User` -> `UserId`) or from type `<Type>Id`
- **Relationships**:
  - **1:1**: Fluent API needed, otherwise EF Core configures 1:n by convention
  - **1:n**: Fluent API not needed, EF Core configures 1:n by convention

## Custom Table and Column Names
```csharp

modelBuilder.Entity<User>()
    .ToTable("User")
    .Property(u => u.Name)
    .HasColumnName("FullName");
```

## Custom Primary Key

```csharp
modelBuilder.Entity<Product>()
    .HasKey(p => p.ArticleNumber);
```

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

---

# EF Core Queries

## Find (Primary Key)

**When?** When you **have the primary key**.

* checks the **Change Tracker** first (already loaded entity -> no SQL)
* otherwise queries the DB by PK
* returns **null** if not found

```csharp
// PK lookup (Id)
var user = db.Users.Find(userId);
```

**Typical**: "Get me the entity with this PK (or null if not found)". Great for lookups by ID.

---

## First / FirstOrDefault

**When?** When **"at least one"** can match and you don’t care which one (or you sort).

* `First(...)`: throws if **none** found
* `FirstOrDefault(...)`: returns **null** if none found

```csharp
// Any user with Gmail (first match, non-deterministic)
var anyGmail = db.Users
    .Where(u => u.Email.EndsWith("@gmail.com"))
    .FirstOrDefault();

// The newest user (OrderBy -> deterministic)
var newest = db.Users
    .OrderByDescending(u => u.CreatedAt)
    .First();
```

**Typical**: "Get me any matching row", "Get me the newest/oldest/cheapest/etc."

---

## Single / SingleOrDefault

**When?** When business rules / constraints say **there must be exactly one**.

* `Single(...)`: throws if **0** or **>1**
* `SingleOrDefault(...)`: returns null if **0**, throws if **>1**

```csharp
// Email is UNIQUE -> exactly one
var user = db.Users .Single(u => u.Email == email);

// Optional unique attribute (0 or 1)
var passport = db.Passports .SingleOrDefault(p => p.Number == number);
```

**Typical:** Login, unique keys (Email, Username), business constraints

---

## Where (filtering)

**What it is:**  
* `Where(...)` returns a query (IQueryable) that represents the filter.
* It **does not execute** until you materialize it (e.g., `ToList()`, `First()`, `Single()`, `Any()`, ...).

**When to use `Where(...)`:**

* **You want multiple rows** (then end with `ToList()` / `ToArray()`)
* **You want to reuse** a base query and apply different terminals (`Any`, `Count`, `First`, …)

```csharp
// Multiple Results
var activeUsers = db.Users
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .ToList();

// Reuse Query
var baseQuery = db.Users.Where(u => u.IsActive); // IQueryable<User>
var anyActive = baseQuery.Any(); // bool
var countActive = baseQuery.Count(); // int
```

## Select (projection/mapping)

**What it is:**
* `Select(...)` returns a query (IQueryable) that represents the mapping from the entity to a different shape (e.g., DTO).
* It **does not execute** until you materialize it (e.g., `ToList()`, `First()`, `Single()`, ...).

**When to use `Select(...)`:**
* **You only need a few columns** -> project to DTO with `Select(...)`
* **You want to transform** the data into a different shape (e.g., combine fields, calculate values, etc.)

```csharp
// Project to DTO
var userDtos = db.Users
    .Where(u => u.IsActive)
    .Select(u => new UserDto(u.Id, u.Name))
    .ToList();
```


## Important LINQ Operators

| Operator       | What it does                                      | Executes Query? | Returns |
|----------------|--------------------------------------------------|-----------------| ----------------|
| `Where(...)`   | Filters rows based on a condition                 | No              | `IQueryable<T>` (deferred) |
| `Select(...)`  | Projects to a different shape (e.g., DTO) | No              | `IQueryable<T>` (deferred) |
| `OrderBy(...)` | Sorts rows by a key                              | No              | `IQueryable<T>` (deferred) |
| `First(...)`   | Returns the first matching row (or throws) | Yes             | `T` (single entity) |
| `Single(...)`  | Returns the single matching row (or throws) | Yes             | `T` (single entity) |
| `ToList()`     | Materializes the query into a list               | Yes             | `List<T>` (all matching entities) |
| `Any()`        | Checks if any rows match the condition            | Yes             | `bool` (true if at least one match) |
| `Count()`      | Counts how many rows match the condition          | Yes             | `int` (number of matching entities) |


## Find vs First vs Single vs Where

**This is equavilent:**

```csharp
var user1 = db.Users.Find(userId); // Checks tracker, then DB by PK
var user2 = db.Users.First(u => u.Id == userId); // Always queries DB, returns first match (throws if none)
var user3 = db.Users.Single(u => u.Id == userId); // Always queries DB, returns single match (throws if none or >1)
var user4 = db.Users.Where(u => u.Id == userId).Single(); // Same as above, but more verbose
```

## Change Tracker

**What it is:**
* EF Core tracks entities inside a `DbContext` (state + changes)
* `Find(pk)`:
  * returns from tracker if already loaded (no DB query)
  * otherwise DB query and track the result
* `First(...)` / `Single(...)`:
  * always execute a DB query, then track the result
* `AsNoTracking()`:
  * always queries DB
  * never tracks results

**Rule:**
* Use tracking for updates.
* Use `AsNoTracking()` for read-only queries.

**Clear tracker:**
* If you want to clear the tracker (e.g., for testing), use:

```csharp
db.ChangeTracker.Clear();
```

---

## Include (eager loading related data)

**What it is:**  
* `Include(...)` tells EF Core to load **navigation properties** in the same query plan.

```csharp
// Load user + their orders
var user = db.Users
    .Include(u => u.Orders)
    .Single(u => u.Id == userId);

// Nested include (ThenInclude)
var user2 = db.Users
    .Include(u => u.Orders)
    .ThenInclude(o => o.Items)
    .Single(u => u.Id == userId);
```

**When you NEED `Include`:**
* You will **access navigation properties**

**When you should NOT use `Include`:**
* You only need a **few columns** -> **project** to DTO with `Select(...)`

```csharp
// Better than Include for list endpoints: project to DTO
var users = db.Users
    .Where(u => u.IsActive)
    .Select(u => new UserListDto(u.Id, u.Name, u.Orders.Count))
    .ToList();
```

# Assert Methods in xUnit

## Basics

* `Assert.Equal(expected, actual)` *(structural equality)*
* `Assert.True(condition)` / `Assert.False(condition)`
* `Assert.Null(x)` / `Assert.NotNull(x)`
* `Assert.InRange(value, min, max)`

## Objects

* `Assert.Same(a, b)` *(reference equality)*
* `Assert.IsType<T>(obj)` *(type check)*

## Collections

* `Assert.Empty(col)` / `Assert.NotEmpty(col)`
* `Assert.Contains(item, col)` / `Assert.DoesNotContain(item, col)`

## Exceptions

* `Assert.Throws<TException>(() => ...)`

