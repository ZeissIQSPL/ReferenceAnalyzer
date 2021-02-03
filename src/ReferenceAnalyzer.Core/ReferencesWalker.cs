using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReferenceAnalyzer.Core.Models;

namespace ReferenceAnalyzer.Core
{
    public class ReferencesWalker : CSharpSyntaxWalker
    {
        private readonly Compilation _compilation;
        private readonly IEnumerable<Func<string, bool>> _ignoreRules;

        public ReferencesWalker(Compilation compilation, IEnumerable<Func<string, bool>> ignoreRules)
        {
            _compilation = compilation;
            _ignoreRules = ignoreRules;
        }

        public ConcurrentBag<ReferenceOccurrence> Occurrences { get; } = new();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            VisitNode(node);

            base.VisitClassDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            VisitNode(node);

            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            VisitNode(node);

            base.VisitStructDeclaration(node);
        }

        private void VisitNode(TypeDeclarationSyntax node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var model = _compilation.GetSemanticModel(node.SyntaxTree);

            var syntaxNodes = node.DescendantNodesAndSelf()
                .Where(d => !d.DescendantNodes().Any());

            foreach (var d in syntaxNodes)
                AnalyzeNode(model, d);
        }

        private void AnalyzeNode(SemanticModel model, SyntaxNode node)
        {
            var invokedSymbol = model.GetSymbolInfo(node).Symbol ??
                                model.GetSymbolInfo(node).CandidateSymbols.FirstOrDefault();

            AddOverloads(model, node);

            var type = invokedSymbol switch
            {
                IMethodSymbol methodSymbol => methodSymbol.ReturnType,
                IPropertySymbol propertySymbol => propertySymbol.Type,
                ITypeSymbol typeSymbol => typeSymbol,
                _ => null
            };

            if (type != null)
            {
                AddOccurrence(type);
                AddDependentTypes(type);
            }

            if (invokedSymbol?.ContainingType != null)
            {
                AddOccurrence(invokedSymbol.ContainingType);
                AddDependentTypes(invokedSymbol.ContainingType);
            }
        }

        private void AddOverloads(SemanticModel model, SyntaxNode node)
        {
            var symbols = model.GetMemberGroup(node);
            if (symbols.Length > 1)
            {
                var parameters = symbols
                    .Cast<IMethodSymbol>()
                    .SelectMany(m => m.Parameters);

                foreach (IParameterSymbol p in parameters)
                    AddOccurrence(p.Type);
            }
        }

        private void AddDependentTypes(ITypeSymbol addedType)
        {
            var current = addedType.BaseType;

            while (current != null)
            {
                AddOccurrence(current);
                current = current.BaseType;
            }

            foreach (var @interface in addedType.AllInterfaces)
                AddOccurrence(@interface);

            if (addedType is INamedTypeSymbol namedType)
                foreach (var arg in namedType.TypeArguments)
                    AddOccurrence(arg);
        }

        private void AddOccurrence(ITypeSymbol current)
        {
            if (current.ContainingAssembly != null && _ignoreRules.All(rule => !rule(current.ContainingAssembly.Name)))
                Occurrences.Add(new ReferenceOccurrence(current, new ReferenceLocation()));
        }
    }
}
