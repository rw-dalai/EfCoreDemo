namespace OneToOneBi.Models;

// BI-DIRECTIONAL 1:1

// DOMAIN MODEL: 1 User <-> 1 Passport

// DATABASE SCHEMA:
/*
 * CREATE TABLE Users (
 *     Id       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     FullName TEXT    NOT NULL
 * );
 *
 * CREATE TABLE Passports (
 *     Id     INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     Number TEXT    NOT NULL,
 *     UserId INTEGER NOT NULL,
 *     FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
 * );
 *
 * -- EF creates this automatically for 1:1 (prevents 1:1 from becoming 1:n)
 * CREATE UNIQUE INDEX IX_Passports_UserId ON Passports (UserId);
 */

// WHERE TO PUT THE FK?

// The FK goes on the side that can't exist without the other.
// -> Passport can't exist without User, so the FK goes on Passport.


// LIFECYCLE OWNERSHIP

// Bidirectional: User has a navigation to Passport, Passport has a navigation back to User.

// *User* is the parent entity (principal).
// *Passport* is the child entity (dependent).

// *Principal* is the Lifecylce owner of the relationship.
// - If the Principal is saved, the Dependent is also saved (cascade insert).
// - If the Principal is deleted, the Dependent is also deleted (cascade delete).



public class User
{
    // --- Properties ---
    
    public int Id { get; set; }

    public string FullName { get; set; }

    public Passport Passport { get; set; }


    // --- EF Ctor ---

    protected User() { }


    // --- Business Ctor ---

    public User(string fullName, Passport passport)
    {
        FullName = fullName;
        
        Passport = passport; // User -> Passport
        passport.User = this; // Passport -> User
    }
}
