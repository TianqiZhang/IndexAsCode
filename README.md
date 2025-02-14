# Index as Code - Azure AI Search Source Generator

A C# Source Generator that eliminates redundancy between Azure AI Search index definitions, data models, and query field references. This project demonstrates how to use .NET source generators to automatically generate strongly-typed C# models and field constants from Azure AI Search index definitions.

## Overview

When working with Azure AI Search, developers often need to maintain three separate but related pieces of code:
1. The index definition (JSON)
2. The corresponding C# model classes
3. Field name constants for querying

This source generator automatically generates #2 and #3 from #1, ensuring they stay in sync and reducing maintenance overhead.

## Features

- Converts Azure AI Search index definitions (JSON) to C# model classes
- Generates strongly-typed field constants for safe query building
- Supports:
  - Basic EDM types (String, Int32, Int64, Double, Boolean, DateTimeOffset)
  - Complex types
  - Collections (e.g., Collection(Edm.String))
  - Geographic points (simplified as string in this demo)

## How It Works

1. Place your index definition in a `.index.json` file in your project
2. Add a reference to the `IndexAsCode.Generator` project
3. Mark the JSON file as "AdditionalFiles" in your project file
4. The source generator automatically creates:
   - Model classes in the `[IndexName].Models` namespace
   - Field constants in the `[IndexName].Fields` namespace

## Example

Given this index definition (`hotels.index.json`):
```json
{
  "name": "hotels",
  "fields": [
    { "name": "HotelId", "type": "Edm.String", "key": true },
    { "name": "HotelName", "type": "Edm.String" },
    { "name": "Tags", "type": "Collection(Edm.String)" },
    { "name": "Address", "type": "Edm.ComplexType", 
      "fields": [
        { "name": "StreetAddress", "type": "Edm.String" },
        { "name": "City", "type": "Edm.String" },
        { "name": "StateProvince", "type": "Edm.String" }
      ]
    }
  ]
}
```

The generator creates corresponding C# classes and constants that you can use in your code:

```csharp
var hotel = new HotelsDocument
{
    HotelId = "123",
    HotelName = "Seaside Resort",
    Tags = new List<string> { "beach", "luxury" },
    Address = new Address
    {
        StreetAddress = "123 Ocean Drive",
        City = "Miami Beach",
        StateProvince = "FL"
    }
};

// Use generated field constants for type-safe field references
Console.WriteLine($"{HotelsFields.HotelName}: {hotel.HotelName}");
Console.WriteLine($"{HotelsFields.Address}/{HotelsFields.AddressCity}: {hotel.Address.City}");
```

## Getting Started

1. Clone this repository
2. Reference the `IndexAsCode.Generator` project in your solution
3. Create your index definition file with `.index.json` extension
4. Build your project - the source generator will create the model classes and field constants

## Project Structure

- `IndexAsCode.Generator/`: The source generator project
- `IndexAsCode.Sample/`: A sample project demonstrating usage
  - `hotels.index.json`: Example index definition
  - `Program.cs`: Usage example

## Created by GitHub Copilot

This project, including this README itself, was created with GitHub Copilot Agent Preview. The initial prompt that led to this implementation was:

```
We need to build a C# Source Generator demo implementing an 'Index as Code' concept for Azure AI Search. The goal is to eliminate the redundancy between index definitions, data models, and query field references.

Requirements:
1. Create a Source Generator that reads Azure Search index definition JSON files from the project
2. Generate two types of C# artifacts:
   - POCO classes matching the index schema (including nested complex types)
   - Constants/properties for field names to use in queries

Sample index definition JSON that demonstrates the structure:
{
  "name": "hotels",
  "fields": [
    { "name": "HotelId", "type": "Edm.String", "key": true },
    { "name": "HotelName", "type": "Edm.String" },
    { "name": "Description", "type": "Edm.String" },
    { "name": "Address", "type": "Edm.ComplexType", 
      "fields": [
        { "name": "StreetAddress", "type": "Edm.String" },
        { "name": "City", "type": "Edm.String" },
        { "name": "StateProvince", "type": "Edm.String" }
      ]
    }
  ]
}

Key technical points:
- Focus on field names and types for the initial implementation
- Handle nested complex types (like the Address field)
- Map Edm.* types to appropriate C# types
- Generate clean, idiomatic C# code
```
I manually updated this section in this README, nothing else.
## License

MIT License
