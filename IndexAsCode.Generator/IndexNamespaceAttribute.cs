using System;

namespace IndexAsCode.Generator;

[AttributeUsage(AttributeTargets.Assembly)]
public class IndexNamespaceAttribute : Attribute
{
    public string Namespace { get; }

    public IndexNamespaceAttribute(string @namespace)
    {
        Namespace = @namespace;
    }
}