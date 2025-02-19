namespace IndexAsCode.Tools;

public class IndexDefinition
{
    public string Name { get; set; } = "";
    public List<FieldDefinition> Fields { get; set; } = new();
}

public class FieldDefinition
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool Key { get; set; }
    public List<FieldDefinition>? Fields { get; set; }
}