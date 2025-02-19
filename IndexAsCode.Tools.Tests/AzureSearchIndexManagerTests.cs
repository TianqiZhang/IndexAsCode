using System.Text.Json;
using Xunit;

namespace IndexAsCode.Tools.Tests;

public class AzureSearchIndexManagerTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    
    public AzureSearchIndexManagerTests()
    {
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    [Fact]
    public async Task CompareWithCurrentAsync_IdenticalIndexes_ReturnsNoDifferences()
    {
        // Arrange
        var json = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""HotelName"", ""type"": ""Edm.String"" }
            ]
        }";

        // Mock the Azure response by using the same JSON
        var mockAzureJson = json;

        // Act
        var result = await CompareJsons(json, mockAzureJson);

        // Assert
        Assert.True(result.Exists);
        Assert.False(result.HasDifferences);
        Assert.Empty(result.Differences);
    }

    [Fact]
    public async Task CompareWithCurrentAsync_AddedField_DetectsAddition()
    {
        // Arrange
        var localJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""HotelName"", ""type"": ""Edm.String"" },
                { ""name"": ""Rating"", ""type"": ""Edm.Int32"" }
            ]
        }";

        var azureJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""HotelName"", ""type"": ""Edm.String"" }
            ]
        }";

        // Act
        var result = await CompareJsons(localJson, azureJson);

        // Assert
        Assert.True(result.HasDifferences);
        Assert.Contains(result.Differences, d => d.Contains("Added") && d.Contains("Rating"));
    }

    [Fact]
    public async Task CompareWithCurrentAsync_RemovedField_DetectsRemoval()
    {
        // Arrange
        var localJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true }
            ]
        }";

        var azureJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""HotelName"", ""type"": ""Edm.String"" }
            ]
        }";

        // Act
        var result = await CompareJsons(localJson, azureJson);

        // Assert
        Assert.True(result.HasDifferences);
        Assert.Contains(result.Differences, d => d.Contains("Removed") && d.Contains("HotelName"));
    }

    [Fact]
    public async Task CompareWithCurrentAsync_ChangedFieldType_DetectsChange()
    {
        // Arrange
        var localJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""Rating"", ""type"": ""Edm.Double"" }
            ]
        }";

        var azureJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""Rating"", ""type"": ""Edm.Int32"" }
            ]
        }";

        // Act
        var result = await CompareJsons(localJson, azureJson);

        // Assert
        Assert.True(result.HasDifferences);
        Assert.Contains("Changed fields[1]/type:", result.Differences);
        Assert.Contains("  From: Edm.Int32", result.Differences);
        Assert.Contains("  To:   Edm.Double", result.Differences);
    }

    [Fact]
    public async Task CompareWithCurrentAsync_ComplexTypeFieldChange_DetectsNestedChanges()
    {
        // Arrange
        var localJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""Address"", ""type"": ""Edm.ComplexType"", 
                  ""fields"": [
                    { ""name"": ""Street"", ""type"": ""Edm.String"" },
                    { ""name"": ""City"", ""type"": ""Edm.String"" },
                    { ""name"": ""Country"", ""type"": ""Edm.String"" }
                  ]
                }
            ]
        }";

        var azureJson = @"{
            ""name"": ""hotels"",
            ""fields"": [
                { ""name"": ""HotelId"", ""type"": ""Edm.String"", ""key"": true },
                { ""name"": ""Address"", ""type"": ""Edm.ComplexType"", 
                  ""fields"": [
                    { ""name"": ""Street"", ""type"": ""Edm.String"" },
                    { ""name"": ""City"", ""type"": ""Edm.String"" }
                  ]
                }
            ]
        }";

        // Act
        var result = await CompareJsons(localJson, azureJson);

        // Assert
        Assert.True(result.HasDifferences);
        Assert.Contains(result.Differences, d => d.Contains("Added") && d.Contains("Country"));
    }

    [Fact]
    public async Task CompareWithCurrentAsync_NonExistentIndex_ReturnsExpectedResult()
    {
        // Act
        var result = await CompareJsons(@"{""name"": ""hotels""}", null);

        // Assert
        Assert.False(result.Exists);
        Assert.False(result.HasDifferences);
        Assert.Equal("Index does not exist in Azure Search", result.Message);
    }

    // Helper method to compare two JSON strings
    private async Task<IndexComparisonResult> CompareJsons(string localJson, string? mockAzureJson)
    {
        var indexName = JsonSerializer.Deserialize<JsonElement>(localJson)
            .GetProperty("name").GetString()!;

        // Create a test double that doesn't make real Azure calls
        var mockManager = new MockAzureSearchIndexManager(mockAzureJson);
        return await mockManager.CompareWithCurrentAsync(indexName, localJson);
    }
}

// Test double that doesn't make real Azure calls
public class MockAzureSearchIndexManager : AzureSearchIndexManager
{
    private readonly string? _mockAzureJson;

    public MockAzureSearchIndexManager(string? mockAzureJson) 
        : base("https://dummy.search.windows.net", "dummy-key")
    {
        _mockAzureJson = mockAzureJson;
    }

    public override Task<string?> GetCurrentIndexJsonAsync(string indexName)
    {
        return Task.FromResult(_mockAzureJson);
    }

    public override Task UpsertIndexAsync(string indexJson)
    {
        // No-op for testing
        return Task.CompletedTask;
    }
}