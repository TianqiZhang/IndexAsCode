namespace IndexAsCode.Tools;

public class IndexComparisonResult
{
    public bool Exists { get; set; }
    public bool HasDifferences { get; set; }
    public string? Message { get; set; }
    public List<string> Differences { get; set; } = new();
}