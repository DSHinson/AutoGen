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
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif
        }
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // used to generate the test console prints with attribute autogen
            // Register a syntax receiver that will be created for each generation pass
            //IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations =
            //    context.SyntaxProvider.CreateSyntaxProvider(
            //        predicate: static (s, _) => IsClassWithAttribute(s, "Autogen"),
            //        transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx,"Autogen"))
            //    .Where(static m => m is not null)!;

            //// Combine the selected classes into a single collection
            //IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> classDeclarationProvider =
            //    classDeclarations.Collect();

            //// Register the source output
            //context.RegisterSourceOutput(classDeclarationProvider, static (spc, classes) => Execute(spc, classes));


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


        private static void Execute(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax> classes)
        {
            foreach (var classDeclaration in classes)
            {
                var className = classDeclaration.Identifier.ToString();
                var message = $"Class {className} is decorated with [autogen] NEW!!!";

                // Log the message to the Visual Studio output window
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "AUTOGEN001",
                        title: "Autogen Class Detected",
                        messageFormat: message,
                        category: "Autogen",
                        DiagnosticSeverity.Info,
                        isEnabledByDefault: true),
                    Location.None));

                var source = $@"
using System;

public static class {className}AutoGenConsole
{{
   public static void print()
    {{
        Console.WriteLine(""{message}"");
    }}
}}
";
                context.AddSource($"{className}AutoGenConsole.g.cs", SourceText.From(source, Encoding.UTF8));

            }
        }


        private void ExecuteController(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax> classes)
        {
            foreach (var classDeclaration in classes)
            {
                var className = classDeclaration.Identifier.ToString();
                var message = $"Class {className} is decorated with [autogen] NEW!!!";

                var source = GetSourceCodeFor(classDeclaration);


                context.AddSource($"{className}Controller.g.cs", SourceText.From(source, Encoding.UTF8));

            }
        }
       private string GetSourceCodeFor(ClassDeclarationSyntax symbol, string? template = null)
        {
            // If template isn't provieded, use default one from embeded resources.
            template ??= GetEmbededResource("Generator.Templates.ControllerTemplate.txt");

            
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
