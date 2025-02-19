using System.Text.Json;
using System.Text.Json.Nodes;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Identity;

namespace IndexAsCode.Tools;

public class AzureSearchIndexManager
{
    private readonly SearchIndexClient _searchClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AzureSearchIndexManager(string endpoint)
    {
        var uri = new Uri(endpoint);
        var credential = new DefaultAzureCredential();
        _searchClient = new SearchIndexClient(uri, credential);
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public virtual async Task<string?> GetCurrentIndexJsonAsync(string indexName)
    {
        try
        {
            var response = await _searchClient.GetIndexAsync(indexName);
            return JsonSerializer.Serialize(response, _jsonOptions);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public virtual async Task UpsertIndexAsync(string indexJson)
    {
        var index = BinaryData.FromString(indexJson).ToObjectFromJson<SearchIndex>();
        await _searchClient.CreateOrUpdateIndexAsync(index);
    }

    public async Task<IndexComparisonResult> CompareWithCurrentAsync(string indexName, string localJson)
    {
        var currentJson = await GetCurrentIndexJsonAsync(indexName);
        if (currentJson == null)
        {
            return new IndexComparisonResult 
            { 
                Exists = false,
                Message = "Index does not exist in Azure Search"
            };
        }

        // Parse both JSONs into JsonNode for comparison
        var localNode = JsonNode.Parse(localJson)!;
        var azureNode = JsonNode.Parse(currentJson)!;

        var differences = new List<string>();
        CompareNodes(localNode, azureNode, "", differences);

        return new IndexComparisonResult
        {
            Exists = true,
            HasDifferences = differences.Count > 0,
            Message = differences.Count > 0 ? 
                "Found differences between local and Azure Search index definitions:" : 
                "Index definitions are identical",
            Differences = differences
        };
    }

    private void CompareNodes(JsonNode localNode, JsonNode azureNode, string path, List<string> differences)
    {
        if (localNode is JsonObject localObj && azureNode is JsonObject azureObj)
        {
            CompareObjects(localObj, azureObj, path, differences);
        }
        else if (localNode is JsonArray localArray && azureNode is JsonArray azureArray)
        {
            CompareArrays(localArray, azureArray, path, differences);
        }
        else if (!JsonNode.DeepEquals(localNode, azureNode))
        {
            differences.Add($"Changed {path}:");
            differences.Add($"  From: {azureNode}");
            differences.Add($"  To:   {localNode}");
        }
    }

    private void CompareObjects(JsonObject localObj, JsonObject azureObj, string path, List<string> differences)
    {
        var allKeys = new HashSet<string>();
        foreach (var prop in localObj)
        {
            allKeys.Add(prop.Key);
        }
        foreach (var prop in azureObj)
        {
            allKeys.Add(prop.Key);
        }

        foreach (var key in allKeys.OrderBy(k => k))
        {
            var fullPath = string.IsNullOrEmpty(path) ? key : $"{path}/{key}";
            
            var hasLocal = localObj.TryGetPropertyValue(key, out var localValue);
            var hasAzure = azureObj.TryGetPropertyValue(key, out var azureValue);

            if (!hasLocal)
            {
                differences.Add($"Removed {fullPath}: {azureValue}");
            }
            else if (!hasAzure)
            {
                differences.Add($"Added {fullPath}: {localValue}");
            }
            else if (localValue != null && azureValue != null)
            {
                CompareNodes(localValue, azureValue, fullPath, differences);
            }
        }
    }

    private void CompareArrays(JsonArray localArray, JsonArray azureArray, string path, List<string> differences)
    {
        var minLength = Math.Min(localArray.Count, azureArray.Count);

        // Compare elements that exist in both arrays
        for (int i = 0; i < minLength; i++)
        {
            CompareNodes(localArray[i]!, azureArray[i]!, $"{path}[{i}]", differences);
        }

        // Check for removed elements
        for (int i = minLength; i < azureArray.Count; i++)
        {
            differences.Add($"Removed {path}[{i}]: {azureArray[i]}");
        }

        // Check for added elements
        for (int i = minLength; i < localArray.Count; i++)
        {
            differences.Add($"Added {path}[{i}]: {localArray[i]}");
        }
    }
}