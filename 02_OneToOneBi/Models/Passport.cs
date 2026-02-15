namespace OneToOneBi.Models;

// *Passport* is the child (dependent) in the one-to-one relationship with *User* (principal).

public class Passport
{
    // --- Properties ---
    
    public int Id { get; set; }

    public string Number { get; set; }
    
    // Navigation property back to User (bidirectional).
    public User User { get; set; }


    // --- EF Ctor ---

    protected Passport() { }


    // --- Business Ctor ---

    public Passport(string number)
    {
        Number = number;
    }
}
