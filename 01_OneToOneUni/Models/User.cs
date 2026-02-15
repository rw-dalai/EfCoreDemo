namespace OneToOneUni.Models;

// UNIDIRECTIONAL 1:1

// DOMAIN MODEL: 1 User -> 1 Profile

// DATABASE SCHEMA:
/*
 * CREATE TABLE Users (
 *     Id       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     FullName TEXT NOT NULL
 *  );
 *
 * CREATE TABLE Profiles (
 *     Id     INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *     Bio    TEXT    NOT NULL,
 *     UserId INTEGER NOT NULL,
 *     FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
 * );
 *
 * -- EF creates this automatically for 1:1 (prevents 1:1 from becoming 1:n)
 * CREATE UNIQUE INDEX IX_Profiles_UserId ON Profiles (UserId);
 */


// WHERE TO PUT THE FK?

// The FK goes on the side that can't exist without the other.
// -> Profile can't exist without User, so the FK goes on Profile.


// LIFECYCLE OWNERSHIP

// Unidirectional: User has a navigation to Profile, Profile has no navigation back to User.

// *User* is the parent entity (principal)
// *Profile* is the child entity (dependent).

// *Principal* is the Lifecylce owner of the relationship.
// - If the Principal is saved, the Dependent is also saved (cascade insert).
// - If the Principal is deleted, the Dependent is also deleted (cascade delete).


public class User
{
    // --- Properties ---
    
    // PK: Id is the primary key by convention.
    public int Id { get; set; }
    
    public string FullName { get; set; }
    
    public Profile Profile { get; set; }
    
    
    // --- EF Ctor ---
    // Used by EF Core to create User instances when loading from the database.
    
    protected User() { }
    
    
    // --- Business Ctor ---
    // Used by the application to create new User instances with all required properties.
    
    public User(string fullName, Profile profile)
    {
        FullName = fullName;
        Profile = profile; // User -> Profile
    }
}
