namespace OneToManyUni.Models;

// *Order* is the child (dependent) in the one-to-many relationship with *User* (principal).

public class Order
{
    // --- Properties ---
    
    public int Id { get; set; }

    public string OrderNumber { get; set; }

    // Navigation property to User (unidirectional).
    public User User { get; set; }


    // --- EF Ctor ---

    protected Order() { }


    // --- Business Ctor ---

    public Order(string orderNumber, User user)
    {
        OrderNumber = orderNumber;
        User = user;
    }
}
