using System.CommandLine;
using System.Text.Json;
using IndexAsCode.Tools;

// Create command-line options
var fileOption = new Option<FileInfo>(
    aliases: ["--index-file", "-f"],
    description: "The path to the index definition JSON file");

var endpointOption = new Option<string>(
    aliases: ["--endpoint", "-e"],
    description: "The Azure Search service endpoint URL");

var keyOption = new Option<string>(
    aliases: ["--key", "-k"],
    description: "The Azure Search admin API key");

var indexNameOption = new Option<string?>(
    aliases: ["--index-name", "-n"],
    description: "The name of the index (optional, defaults to name in JSON file)",
    getDefaultValue: () => null);

// Create commands
var diffCommand = new Command("diff", "Compare local index definition with Azure Search")
{
    fileOption,
    endpointOption,
    keyOption,
    indexNameOption
};

var updateCommand = new Command("update", "Create or update index in Azure Search")
{
    fileOption,
    endpointOption,
    keyOption,
    indexNameOption
};

// Set handlers
diffCommand.SetHandler(async (file, endpoint, key, indexName) =>
{
    try
    {
        var json = await File.ReadAllTextAsync(file.FullName);
        
        // If index name is provided, we need to parse just to get or update the name
        if (!string.IsNullOrEmpty(indexName))
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            var mutableDoc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
            mutableDoc["name"] = JsonSerializer.SerializeToElement(indexName);
            json = JsonSerializer.Serialize(mutableDoc, new JsonSerializerOptions { WriteIndented = true });
        }
        else
        {
            // Just parse to get the name and validate JSON
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            if (!jsonElement.TryGetProperty("name", out var nameElement) || nameElement.GetString() == null)
            {
                throw new InvalidOperationException("Index name is required in the JSON definition");
            }
            indexName = nameElement.GetString();
        }

        var manager = new AzureSearchIndexManager(endpoint, key);
        var result = await manager.CompareWithCurrentAsync(indexName!, json);

        if (!result.Exists)
        {
            Console.WriteLine(result.Message);
            return;
        }

        if (!result.HasDifferences)
        {
            Console.WriteLine("Index definitions are identical.");
            return;
        }

        Console.WriteLine(result.Message);
        foreach (var diff in result.Differences)
        {
            Console.WriteLine(diff);
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Environment.Exit(1);
    }
}, fileOption, endpointOption, keyOption, indexNameOption);

updateCommand.SetHandler(async (file, endpoint, key, indexName) =>
{
    try
    {
        var json = await File.ReadAllTextAsync(file.FullName);
        
        // If index name is provided, update it in the JSON
        if (!string.IsNullOrEmpty(indexName))
        {
            var mutableDoc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
            mutableDoc["name"] = JsonSerializer.SerializeToElement(indexName);
            json = JsonSerializer.Serialize(mutableDoc, new JsonSerializerOptions { WriteIndented = true });
        }
        else
        {
            // Just validate that name exists
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            if (!jsonElement.TryGetProperty("name", out var nameElement) || nameElement.GetString() == null)
            {
                throw new InvalidOperationException("Index name is required in the JSON definition");
            }
        }

        var manager = new AzureSearchIndexManager(endpoint, key);
        await manager.UpsertIndexAsync(json);
        
        var indexNameFromJson = JsonSerializer.Deserialize<JsonElement>(json)
            .GetProperty("name").GetString();
        Console.WriteLine($"Successfully created/updated index '{indexNameFromJson}'");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Environment.Exit(1);
    }
}, fileOption, endpointOption, keyOption, indexNameOption);

var rootCommand = new RootCommand("Azure Search Index Management Tool")
{
    diffCommand,
    updateCommand
};

return await rootCommand.InvokeAsync(args);