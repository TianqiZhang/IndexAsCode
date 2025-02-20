using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace IndexAsCode.Generator;

[Generator]
public class IndexGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get the namespace from assembly attributes
        var namespaceOption = context.CompilationProvider
            .Select((compilation, _) =>
            {
                // Look for our namespace attribute
                var attributes = compilation.Assembly.GetAttributes();
                var namespaceAttr = attributes.FirstOrDefault(a => 
                    a.AttributeClass?.ToDisplayString() == typeof(IndexNamespaceAttribute).FullName);

                // If attribute found, get its value
                if (namespaceAttr != null && 
                    namespaceAttr.ConstructorArguments.Length > 0 && 
                    namespaceAttr.ConstructorArguments[0].Value is string ns)
                {
                    return ns;
                }

                return null;
            });

        // Get the json files
        var jsonFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".index.json", StringComparison.OrdinalIgnoreCase));

        var indexDefinitions = jsonFiles.Select((file, cancellationToken) =>
        {
            var content = file.GetText(cancellationToken)!.ToString();
            return JsonSerializer.Deserialize<IndexDefinition>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        });

        // Combine the namespace option with index definitions
        var combined = indexDefinitions.Combine(namespaceOption);
        
        context.RegisterSourceOutput(combined, 
            (context, tuple) => GenerateCode(context, tuple.Left, tuple.Right));
    }

    private void GenerateCode(SourceProductionContext context, IndexDefinition? indexDef, string? customNamespace)
    {
        if (indexDef == null) return;

        var modelSource = GenerateModelClass(indexDef, customNamespace);
        var fieldsSource = GenerateFieldConstants(indexDef, customNamespace);

        context.AddSource($"{indexDef.Name}Model.g.cs", SourceText.From(modelSource, Encoding.UTF8));
        context.AddSource($"{indexDef.Name}Fields.g.cs", SourceText.From(fieldsSource, Encoding.UTF8));
    }

    private string GenerateModelClass(IndexDefinition index, string? customNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();

        sb.AppendLine($"namespace {customNamespace ?? index.Name}.Models");
        sb.AppendLine("{");
        
        // Generate all complex types first
        foreach (var field in index.Fields)
        {
            if (field.Type == "Edm.ComplexType" && field.Fields != null)
            {
                GenerateComplexType(sb, field);
                sb.AppendLine();
            }
        }

        // Generate main model class
        GenerateMainClass(sb, index);
        
        sb.AppendLine("}");
        return sb.ToString();
    }

    private void GenerateComplexType(StringBuilder sb, FieldDefinition complexField)
    {
        var className = CapitalizeFirstLetter(complexField.Name);
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");

        foreach (var field in complexField.Fields!)
        {
            var csType = GetCSharpType(field);
            var propertyName = CapitalizeFirstLetter(field.Name);
            // Use JsonPropertyName attribute to maintain original field name
            sb.AppendLine($"        [System.Text.Json.Serialization.JsonPropertyName(\"{field.Name}\")]");
            sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}");

            if (field.Type == "Edm.ComplexType" && field.Fields != null)
            {
                GenerateComplexType(sb, field);
            }
        }

        sb.AppendLine("    }");
    }

    private void GenerateMainClass(StringBuilder sb, IndexDefinition index)
    {
        var className = CapitalizeFirstLetter(index.Name) + "Document";
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");

        foreach (var field in index.Fields)
        {
            var csType = GetCSharpType(field);
            var propertyName = CapitalizeFirstLetter(field.Name);
            // Use JsonPropertyName attribute to maintain original field name
            sb.AppendLine($"        [System.Text.Json.Serialization.JsonPropertyName(\"{field.Name}\")]");
            sb.AppendLine($"        public {csType} {propertyName} {{ get; set; }}");
        }

        sb.AppendLine("    }");
    }

    private string GenerateFieldConstants(IndexDefinition index, string? customNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {customNamespace ?? index.Name}.Fields");
        sb.AppendLine("{");
        
        // Use capitalized name for consistency with document class
        var className = char.ToUpperInvariant(index.Name[0]) + index.Name.Substring(1) + "Fields";
        sb.AppendLine($"    public static class {className}");
        sb.AppendLine("    {");

        foreach (var field in index.Fields)
        {
            GenerateFieldConstant(sb, field, "", field.Name);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private void GenerateFieldConstant(StringBuilder sb, FieldDefinition field, string prefix, string propertyName)
    {
        var fieldConstantName = CapitalizeFirstLetter(propertyName);
        var fullPath = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}/{field.Name}";
        sb.AppendLine($"        public const string {fieldConstantName} = \"{fullPath}\";");

        if (field.Type == "Edm.ComplexType" && field.Fields != null)
        {
            foreach (var subField in field.Fields)
            {
                var subPropertyName = $"{propertyName}{CapitalizeFirstLetter(subField.Name)}";
                GenerateFieldConstant(sb, subField, fullPath, subPropertyName);
            }
        }
    }

    private string GetCSharpType(FieldDefinition field)
    {
        if (field.Type.StartsWith("Collection("))
        {
            var innerType = field.Type.Substring(11, field.Type.Length - 12); // Remove Collection( and )
            return $"List<{GetCSharpTypeInternal(innerType, field.Name)}>";
        }
        return GetCSharpTypeInternal(field.Type, field.Name);
    }

    private string GetCSharpTypeInternal(string type, string fieldName) => type switch
    {
        "Edm.String" => "string",
        "Edm.Int32" => "int",
        "Edm.Int64" => "long",
        "Edm.Double" => "double",
        "Edm.Boolean" => "bool",
        "Edm.DateTimeOffset" => "DateTimeOffset",
        "Edm.GeographyPoint" => "string", // Simplified for demo
        "Edm.ComplexType" => CapitalizeFirstLetter(fieldName),
        _ => "string"
    };

    private string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }
}

public class IndexDefinition
{
    public string Name { get; set; } = "";
    public List<FieldDefinition> Fields { get; set; } = new List<FieldDefinition>();
}

public class FieldDefinition
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool Key { get; set; }
    public List<FieldDefinition>? Fields { get; set; }
}
