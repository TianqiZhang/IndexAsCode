using Hotels.Models;
using Hotels.Fields;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// Create a new hotel using the generated model classes
var hotel = new HotelsDocument
{
    HotelId = "123",
    HotelName = "Seaside Resort",
    Description = "A beautiful beachfront resort",
    Address = new Address
    {
        StreetAddress = "123 Ocean Drive",
        City = "Miami Beach",
        StateProvince = "FL"
    }
};

// Demonstrate using the generated field constants
Console.WriteLine($"Accessing hotel using field constants:");
Console.WriteLine($"{HotelsFields.HotelName}: {hotel.HotelName}");
Console.WriteLine($"{HotelsFields.Address}/{HotelsFields.AddressStreetAddress}: {hotel.Address.StreetAddress}");
