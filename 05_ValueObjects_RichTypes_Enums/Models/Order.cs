// DATABASE SCHEMA:

/*
 *   CREATE TABLE Orders (
 *       Id                       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
 *       Status                   TEXT    NOT NULL,
 *       CustomerEmail            TEXT    NOT NULL,
 *       ShippingAddress_Street   TEXT    NOT NULL,
 *       ShippingAddress_City     TEXT    NOT NULL,
 *       ShippingAddress_Zip      TEXT    NOT NULL
 *   );
*/

namespace Conversions.Models;

public class Order
{
    // --- Properties ---

    public int Id { get; set; }

    public OrderStatus Status { get; set; } // Enum

    public Email CustomerEmail { get; set; } // Rich Type

    public Address ShippingAddress { get; set; } // Value Object


    // --- EF Ctor ---

    protected Order() { }


    // --- Business Ctor ---

    public Order(OrderStatus status, Email customerEmail, Address shippingAddress)
    {
        Status = status;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
    }
}
