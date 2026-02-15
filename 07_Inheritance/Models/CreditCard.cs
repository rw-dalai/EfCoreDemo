namespace Inheritance.Models;

public class CreditCard : PaymentMethod
{
    // --- Properties ---

    public string CardNumber { get; set; }

    public string ExpiryDate { get; set; }


    // --- EF Ctor ---

    protected CreditCard() { }


    // --- Business Ctor ---

    public CreditCard(string owner, string cardNumber, string expiryDate)
    {
        Owner = owner;
        CardNumber = cardNumber;
        ExpiryDate = expiryDate;
    }
}
