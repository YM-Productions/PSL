using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.SubModules;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SourceGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<System.Collections.Immutable.ImmutableArray<ITypeSymbol?>> classes = context
            .SyntaxProvider.CreateSyntaxProvider(HasAttributes, CheckAttribute)
            .Where(cl => cl is not null)
            .Collect();

        context.RegisterSourceOutput(classes, GenerateSource);
    }

    private bool HasAttributes(SyntaxNode node, CancellationToken token)
    {
        if (node is ClassDeclarationSyntax nodeSyntax)
        {
            return nodeSyntax.AttributeLists.Count > 0;
        }
        return false;
    }

    private ITypeSymbol? CheckAttribute(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.AttributeLists.Where(a => a.Attributes.Any(b => b.Name.NormalizeWhitespace().ToFullString()
            .Contains("GenerateUI"))).Count() > 0)
        {
            return context.SemanticModel.GetDeclaredSymbol(classDeclaration) as ITypeSymbol;
        }
        return null;
    } 

    public void GenerateSource(SourceProductionContext context, System.Collections.Immutable.ImmutableArray<ITypeSymbol?> array)
    {
        StringBuilder logger = new();
        logger.AppendLine("Starting Generating Process!");

        if (!array.IsDefaultOrEmpty)
        {
            foreach (ITypeSymbol type in array)
            {
                foreach (AttributeData attr in type.GetAttributes())
                {
                    logger.AppendLine($"-----------{type.Name}-----------");

                    if (attr.AttributeClass?.Name == "GenerateUIAttribute")
                    {
                        logger.AppendLine($"Starting to generate InspectUI for {type.Name}");
                        UIGenerator.GenerateUI(context, type, attr, out string output);
                        logger.Append(output);
                        logger.AppendLine($"Ended to generate InspectUI for {type.Name}");
                    }

                    logger.AppendLine("----------------------------------");
                }
            }

        }

        logger.AppendLine("Finished Generating Process!");
        context.AddSource("00Log.ygen.txt.cs", logger.ToString());
    }
}