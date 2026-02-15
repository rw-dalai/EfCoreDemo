namespace OneToOneUni.Models;

// *Profile* is the child (dependent) in the one-to-one relationship with *User* (principal).

public class Profile
{
    // --- Properties ---
    
    // PK: Id is the primary key by convention.
    public int Id { get; init; }

    public string Bio { get; init; }
    
    // No navigation property back to User (unidirectional).
    
    // --- EF Ctor ---
    
    protected Profile() { }
    

    // --- Business Ctor ---
    
    public Profile(string bio)
    {
        Bio = bio;
    }
}
