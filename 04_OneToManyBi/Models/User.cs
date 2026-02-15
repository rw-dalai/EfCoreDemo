namespace OneToManyBi.Models;

// BIDIRECTIONAL 1:N

// DOMAIN MODEL: 1 User <-> n Orders

// DATABASE SCHEMA:

/*
 * CREATE TABLE Users (
 *     Id       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     FullName TEXT    NOT NULL
 * );
 *
 * CREATE TABLE Orders (
 *     Id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     OrderNumber TEXT    NOT NULL,
 *     UserId      INTEGER NOT NULL,
 *     FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
 * );
 *
 * CREATE INDEX IX_Orders_UserId ON Orders (UserId);
 */


// WHERE TO PUT THE FK?

// The FK goes on the n side (the "many" side) of the relationship.
// -> Order is the "many" side, so the FK goes on Order.


// LIFECYCLE OWNERSHIP

// Bidirectional: User has a List<Order> collection, Order has a navigation back to User.

// *User* is the parent entity (principal).
// *Order* is the child entity (dependent).

// *Principal* is the Lifecycle owner of the relationship.
// - If the Principal is saved, the Dependent is also saved (cascade insert).
// - If the Principal is deleted, the Dependent is also deleted (cascade delete).



public class User
{
    // --- Properties ---
    
    public int Id { get; set; }

    public string FullName { get; set; }

    // Collection navigation: one User has many Orders (bidirectional).
    private readonly List<Order> _orders = [];
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();


    // --- EF Ctor ---

    protected User() { }


    // --- Business Ctor ---

    public User(string fullName)
    {
        FullName = fullName;
    }

    // --- Methods ---

    public void AddOrder(Order order)
    {
        _orders.Add(order);     // User -> Order
        order.User = this;     // Order -> User
    }
}
