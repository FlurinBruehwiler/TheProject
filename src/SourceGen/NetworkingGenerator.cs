using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGen;

public static class NetworkingGenerator
{
    public static void Generate(string interfaceFilePath)
    {
        var interfaceFile = File.ReadAllText(interfaceFilePath);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(interfaceFile);
        var interfaces = new List<InterfaceDeclarationSyntax>();
        new InterfaceMemberFinder(interfaces).Visit(tree.GetRoot());

        foreach (var i in interfaces)
        {
            GenerateInterfaceImplementation(i);
        }
    }

    public static void GenerateInterfaceImplementation(InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        var sb = new SourceBuilder();

        var interfaceName = interfaceDeclarationSyntax.Identifier.Text;

        Debug.Assert(interfaceName.StartsWith("I"));

        var className = interfaceName.Substring(1);

        sb.AppendLine($"public class {className} : {interfaceName}");
        sb.AppendLine("{");
        sb.AddIndent();

        foreach (MethodDeclarationSyntax interfaceMember in interfaceDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
        {
            //todo
        }

        sb.AppendLine("}");
        sb.RemoveIndent();
    }
}

public class InterfaceMemberFinder(List<InterfaceDeclarationSyntax> interfaces) : CSharpSyntaxWalker
{
    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        interfaces.Add(node);
    }
}