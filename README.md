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

- `IndexAsCode.Generator/`: The source generator project targeting .NET Standard 2.0
- `IndexAsCode.Generator.Sample/`: A sample project demonstrating usage (targeting .NET 9.0)
  - `hotels.index.json`: Example index definition
  - `Program.cs`: Usage example

## Configuration

The project requires the following configuration in your `.csproj` file:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>

<ItemGroup>
  <AdditionalFiles Include="**/*.index.json" />
</ItemGroup>
```

This configuration ensures:
- Source generator output files are emitted for debugging
- Index definition JSON files are properly included as additional files

## Namespace Customization

You can customize the namespace of the generated code by adding an assembly-level attribute:

```csharp
[assembly: IndexAsCode.Generator.IndexNamespace("MyCompany.Search")]
```

This will generate the model classes and field constants in the specified namespace instead of using the index name as the namespace.

## Index Management Tool

The solution includes an index management tool that helps maintain synchronization between your local index definitions and Azure Search. The tool provides two main commands:

### Comparing Index Definitions (diff)
```bash
IndexAsCode.Tools diff --index-file path/to/hotels.index.json --endpoint https://your-search-service.search.windows.net --key your-admin-key
```

This command will show differences between your local index definition and the one in Azure Search:

```
Found differences between local and Azure Search index definitions:
Changed fields/1/searchable:
  From: true
  To:   false
Added fields/2/filterable: true
Removed fields/3/synonymMaps: ["location-synonyms"]
```

### Updating Index Definitions (update)
```bash
IndexAsCode.Tools update --index-file path/to/hotels.index.json --endpoint https://your-search-service.search.windows.net --key your-admin-key
```

This command will create a new index or update an existing one in Azure Search based on your local definition.

You can optionally specify a different index name with `--index-name`:
```bash
IndexAsCode.Tools update --index-file hotels.index.json --index-name hotels-staging --endpoint ... --key ...
```

## Technical Implementation

The source generator is implemented using:
- .NET Standard 2.0 for maximum compatibility
- Microsoft.CodeAnalysis.CSharp 4.8.0 for source generation
- System.Text.Json 8.0.0 for JSON parsing
- Case-insensitive JSON property name matching
- Roslyn Incremental Generator API for optimal performance

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
