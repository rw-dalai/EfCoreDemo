namespace Inheritance.Models;

public class PayPal : PaymentMethod
{
    // --- Properties ---

    public string Email { get; set; }


    // --- EF Ctor ---

    protected PayPal() { }


    // --- Business Ctor ---

    public PayPal(string owner, string email)
    {
        Owner = owner;
        Email = email;
    }
}
