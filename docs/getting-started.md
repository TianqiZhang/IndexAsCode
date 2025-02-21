# Getting Started with Index as Code

## Prerequisites
- .NET SDK 9.0 or later
- An Azure AI Search service
- Basic understanding of Azure AI Search concepts

## Initial Setup

1. Clone this repository
2. Add the following NuGet package references to your project:
   ```xml
   <ItemGroup>
     <PackageReference Include="IndexAsCode.Generator" Version="1.0.0" />
     <PackageReference Include="Azure.Search.Documents" Version="11.5.1" />
   </ItemGroup>
   ```
3. Create your index definition file with `.index.json` extension
4. Configure your project as shown below

## Project Configuration

Add this to your `.csproj` file:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>

<ItemGroup>
  <AdditionalFiles Include="**/*.index.json" />
</ItemGroup>
```

## Development Workflow

1. **Define Your Index**
   Create an index definition file (e.g., `search.index.json`):
   ```json
   {
     "name": "search",
     "fields": [
       { "name": "id", "type": "Edm.String", "key": true },
       { "name": "title", "type": "Edm.String", "searchable": true }
     ]
   }
   ```

2. **Generate Code**
   - Build your project
   - The generator creates:
     - `SearchDocument.cs` - Your model class
     - `SearchFields.cs` - Field name constants

3. **Use Generated Code**
   ```csharp
   // Create documents
   var doc = new SearchDocument 
   { 
       Id = "1", 
       Title = "Example" 
   };

   // Use field constants in queries
   var results = await searchClient.SearchAsync<SearchDocument>(
       "query",
       new SearchOptions 
       { 
           SearchFields = { SearchFields.Title } 
       });
   ```

## Local Development

1. **Test Index Changes**
   Before deploying, check if your changes are valid:
   ```bash
   IndexAsCode.Tools diff -f search.index.json -e $SEARCH_ENDPOINT
   ```

2. **Update Local Development Index**
   Apply changes to your development environment:
   ```bash
   IndexAsCode.Tools update -f search.index.json -e $SEARCH_ENDPOINT -n search-dev
   ```

## CI/CD Integration

1. **Add Deployment Scripts**
   - Copy `scripts/update-index.ps1` (Windows) or `scripts/update-index.sh` (Linux) to your deployment folder
   - Add script execution to your CI/CD pipeline

2. **Configure Environments**
   Set up different index names for each environment:
   ```yaml
   # azure-pipelines.yml example
   stages:
   - stage: Deploy_Dev
     variables:
       indexName: search-dev
   - stage: Deploy_Prod
     variables:
       indexName: search
   ```

3. **Run Deployment**
   ```bash
   ./update-index.sh -f search.index.json -e $SEARCH_ENDPOINT -n $indexName
   ```

## Advanced Configuration

### Custom Namespace
Add this to any .cs file in your project:
```csharp
[assembly: IndexAsCode.Generator.IndexNamespace("MyCompany.Search")]
```

### Multiple Indexes
You can have multiple index definitions:
```
/indexes
  ├── products.index.json
  ├── customers.index.json
  └── orders.index.json
```

## Next Steps

- Read the [Technical Overview](technical-overview.md) for best practices
- See the [Index Management Tool](index-management-tool.md) for detailed CLI usage
- Check the sample project in `IndexAsCode.Generator.Sample/`