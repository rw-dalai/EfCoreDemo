// TPH — Table-Per-Hierarchy (Single Table Inheritance)

// All subclasses share ONE table. EF adds a discriminator column ("PaymentType").

// Parent class columns are NOT NULLable.
// Subclass-specific columns are NULLable.

/*
    CREATE TABLE PaymentMethods (
        Id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
        Owner       TEXT    NOT NULL,
        PaymentType TEXT    NOT NULL,
        CardNumber  TEXT    NULL,
        ExpiryDate  TEXT    NULL,
        Email       TEXT    NULL
    );
*/

namespace Inheritance.Models;

public abstract class PaymentMethod
{
    public int Id { get; set; }

    public string Owner { get; set; }
}
