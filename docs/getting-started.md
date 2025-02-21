# Getting Started with Index as Code

## Prerequisites
- .NET SDK 9.0 or later
- An Azure AI Search service

## Installation

1. Clone this repository
2. Reference the `IndexAsCode.Generator` project in your solution
3. Create your index definition file with `.index.json` extension
4. Build your project - the source generator will create the model classes and field constants

## Project Configuration

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