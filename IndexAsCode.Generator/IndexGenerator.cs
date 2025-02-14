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
        #if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
        #endif

        // Get the MSBuild property directly from analyzer config
        IncrementalValueProvider<string?> namespaceOption = context.AnalyzerConfigOptionsProvider
            .Select((config, _) =>
            {
                // Try get the value with the exact property name
                config.GlobalOptions.TryGetValue("build_property.IndexAsCodeNamespace", out var customNamespace);
                return customNamespace;
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

        // Combine the namespace option with index definitions and add diagnostics
        var combined = indexDefinitions.Combine(namespaceOption);
        
        context.RegisterSourceOutput(combined, 
            (context, tuple) => 
            {
                // Add diagnostic info about what namespace we're using
                var message = tuple.Right != null 
                    ? $"Using namespace: {tuple.Right}" 
                    : "No custom namespace found, using default";
                    
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "IAC002",
                        "Namespace Info",
                        message,
                        "IndexAsCode",
                        DiagnosticSeverity.Info,
                        true),
                    Location.None));
                
                GenerateCode(context, tuple.Left, tuple.Right);
            });
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
        sb.AppendLine($"    public class {complexField.Name}");
        sb.AppendLine("    {");

        foreach (var field in complexField.Fields!)
        {
            var csType = GetCSharpType(field);
            sb.AppendLine($"        public {csType} {field.Name} {{ get; set; }}");

            if (field.Type == "Edm.ComplexType" && field.Fields != null)
            {
                GenerateComplexType(sb, field);
            }
        }

        sb.AppendLine("    }");
    }

    private void GenerateMainClass(StringBuilder sb, IndexDefinition index)
    {
        var className = char.ToUpperInvariant(index.Name[0]) + index.Name.Substring(1) + "Document";
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");

        foreach (var field in index.Fields)
        {
            var csType = GetCSharpType(field);
            sb.AppendLine($"        public {csType} {field.Name} {{ get; set; }}");
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
        var fieldName = char.ToUpperInvariant(propertyName[0]) + propertyName.Substring(1);
        var fullPath = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}/{field.Name}";
        sb.AppendLine($"        public const string {fieldName} = \"{field.Name}\";");

        if (field.Type == "Edm.ComplexType" && field.Fields != null)
        {
            foreach (var subField in field.Fields)
            {
                var subPropertyName = $"{propertyName}{char.ToUpperInvariant(subField.Name[0])}{subField.Name.Substring(1)}";
                GenerateFieldConstant(sb, subField, field.Name, subPropertyName);
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
        "Edm.ComplexType" => fieldName,
        _ => "string"
    };
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
