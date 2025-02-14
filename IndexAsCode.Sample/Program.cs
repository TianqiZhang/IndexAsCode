using MyCompany.Search.Models;
using MyCompany.Search.Fields;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// Create a new hotel using the generated model classes
var hotel = new HotelsDocument
{
    HotelId = "123",
    HotelName = "Seaside Resort",
    Description = "A beautiful beachfront resort",
    Tags = new List<string> { "beach", "luxury", "family-friendly", "spa" },
    Address = new Address
    {
        StreetAddress = "123 Ocean Drive",
        City = "Miami Beach",
        StateProvince = "FL"
    }
};

// Demonstrate using the generated field constants
Console.WriteLine($"Accessing hotel using field constants:");
Console.WriteLine($"{HotelsFields.HotelId}: {hotel.HotelId}");
Console.WriteLine($"{HotelsFields.Address}/{HotelsFields.AddressStreetAddress}: {hotel.Address.StreetAddress}");
Console.WriteLine($"{HotelsFields.Tags}: {string.Join(", ", hotel.Tags)}");
