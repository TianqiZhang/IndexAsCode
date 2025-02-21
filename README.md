# Index as Code - Azure AI Search Source Generator

A C# Source Generator that eliminates redundancy between Azure AI Search index definitions, data models, and query field references. This project demonstrates how to use .NET source generators to automatically generate strongly-typed C# models and field constants from Azure AI Search index definitions.

## Overview

When working with Azure AI Search, developers often need to maintain three separate but related pieces of code:
1. The index definition (JSON)
2. The corresponding C# model classes
3. Field name constants for querying

This source generator automatically generates #2 and #3 from #1, ensuring they stay in sync and reducing maintenance overhead.

## Quick Example

Given this index definition (`hotels.index.json`):
```json
{
  "name": "hotels",
  "fields": [
    { "name": "HotelId", "type": "Edm.String", "key": true },
    { "name": "HotelName", "type": "Edm.String" },
    { "name": "Address", "type": "Edm.ComplexType", 
      "fields": [
        { "name": "City", "type": "Edm.String" },
        { "name": "StateProvince", "type": "Edm.String" }
      ]
    }
  ]
}
```

The generator creates C# models and constants you can use:

```csharp
var hotel = new HotelsDocument
{
    HotelId = "123",
    HotelName = "Seaside Resort",
    Address = new Address
    {
        City = "Miami Beach",
        StateProvince = "FL"
    }
};

// Use generated field constants
Console.WriteLine($"{HotelsFields.HotelName}: {hotel.HotelName}");
```

## Documentation

- [Getting Started Guide](docs/getting-started.md)
- [Index Management Tool](docs/index-management-tool.md)
- Deployment Scripts
  - [PowerShell](docs/scripts/update-index.ps1)
  - [Bash](docs/scripts/update-index.sh)

## Project Structure

- `IndexAsCode.Generator/`: The source generator project (.NET Standard 2.0)
- `IndexAsCode.Generator.Sample/`: Sample project demonstrating usage (.NET 9.0)
- `IndexAsCode.Tools/`: Command-line tool for index management
- `IndexAsCode.Tools.Tests/`: Unit tests for the tools project

## Features

- Generates C# models from Azure AI Search index definitions
- Supports:
  - Basic EDM types (String, Int32, Int64, Double, Boolean, DateTimeOffset)
  - Complex types
  - Collections
  - Geographic points
- Command-line tools for index management and deployment
- Automated deployment scripts for CI/CD pipelines

## License

MIT License

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
