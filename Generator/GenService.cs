using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Generator
{
    [Generator]
    public class GenService : IIncrementalGenerator
    {
        public GenService()
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // used to generate the test console prints with attribute autogen
            // Register a syntax receiver that will be created for each generation pass
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations =
                context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassWithAttribute(s, "AutoGenRepository"),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx, "AutoGenRepository"))
                .Where(static m => m is not null)!;

            // Combine the selected classes into a single collection
            IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> classDeclarationProvider =
                classDeclarations.Collect();

            // Register the source output
            context.RegisterSourceOutput(classDeclarationProvider, (spc, classes) => ExecuteRepository(spc, classes));


            //used to generate controllers with attribute autogenController
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarationsController =
                context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => IsClassWithAttribute(s, "AutoGenController"),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx, "AutoGenController"))
                .Where(static m => m is not null)!;

            // Combine the selected classes into a single collection
            IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> classDeclarationControllerProvider =
                classDeclarationsController.Collect();

            // Register the source output
           context.RegisterSourceOutput(classDeclarationControllerProvider, (spc, classes) => ExecuteController(spc, classes));
        }
        private static bool IsClassWithAttribute(SyntaxNode node, string attribute)
        {
            // Check if the node is a class declaration with attributes
            if (node is ClassDeclarationSyntax classDeclaration)
            {
                return classDeclaration.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(a => a.Name.ToString() == attribute);
            }
            return false;
        }

        private static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context,string namePredicate)
        {
            // We already know the node is a ClassDeclarationSyntax
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            // Perform semantic model checks (e.g., to confirm the attribute is [autogen])
            foreach (var attributeList in classDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var name = attribute.Name.ToString();
                    if (name == namePredicate)
                    {
                        return classDeclaration;
                    }
                }
            }

            return null!;
        }


        private void ExecuteRepository(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax> classes)
        {
            foreach (var classDeclaration in classes)
            {
                var className = classDeclaration.Identifier.ToString();

                var source = GetSourceCodeFor(classDeclaration, "Generator.Templates.RepositoryTemplate.txt");


                context.AddSource($"{className}Repository.g.cs", SourceText.From(source, Encoding.UTF8));

            }
        }


        private void ExecuteController(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax> classes)
        {
            foreach (var classDeclaration in classes)
            {
                var className = classDeclaration.Identifier.ToString();

                var source = GetSourceCodeFor(classDeclaration, "Generator.Templates.ControllerTemplate.txt");


                context.AddSource($"{className}Controller.g.cs", SourceText.From(source, Encoding.UTF8));

            }
        }
       private string GetSourceCodeFor(ClassDeclarationSyntax symbol, string templatePath)
        {
            // If template isn't provieded, use default one from embeded resources.
            string template = GetEmbededResource(templatePath);

            
            // Can't use scriban at the moment, make it manually for now.
            return template
                .Replace("{{ClassName}}", symbol.Identifier.ToString())
                .Replace("{{Namespace}}", GetNamespaceRecursively(syntaxNode: symbol?.Parent));
        }

        private string GetEmbededResource(string path)
        {
            using var stream = GetType().Assembly.GetManifestResourceStream(path);

            using var streamReader = new StreamReader(stream);

            return streamReader.ReadToEnd();
        }

        string GetNamespaceRecursively(SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
            {
                return string.Empty;
            }

            // If the node is a namespace declaration, get its name and recursively get the parent namespace
            if (syntaxNode is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                var parentNamespace = GetNamespaceRecursively(namespaceDeclaration?.Parent);
                return string.IsNullOrEmpty(parentNamespace) ?
                       namespaceDeclaration.Name.ToString() :
                       parentNamespace + "." + namespaceDeclaration.Name;
            }

            // Otherwise, move to the parent node
            return GetNamespaceRecursively(syntaxNode?.Parent);
        }
    }
}
