using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReferenceAnalyzer.Core
{
    public class ReferencesWalker : CSharpSyntaxWalker
    {
        private readonly Compilation _compilation;

        public ReferencesWalker(Compilation compilation)
        {
            _compilation = compilation;
        }

        public ConcurrentBag<ReferenceOccurrence> Occurrences { get; } = new ConcurrentBag<ReferenceOccurrence>();

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

            var syntaxNodes = node.DescendantNodesAndSelf().Where(d => !d.DescendantNodes().Any());
            foreach (var d in syntaxNodes)
            {
                var invokedSymbol = model.GetSymbolInfo(d).Symbol ??
                                    model.GetSymbolInfo(d).CandidateSymbols.FirstOrDefault();

                //method overloads handling
                if (model.GetMemberGroup(d).Length > 1)
                {
                    foreach (var symbol in model.GetMemberGroup(d))
                    {
                        var m = (IMethodSymbol) symbol;
                        foreach (var p in m.Parameters)
                        {
                            Occurrences.Add(new ReferenceOccurrence(p.Type, new ReferenceLocation()));
                        }
                    }
                }

                if (invokedSymbol is IMethodSymbol methodSymbol)
                {
                    Occurrences.Add(new ReferenceOccurrence(methodSymbol.ReturnType, new ReferenceLocation()));

                    AddDependentTypes(methodSymbol.ReturnType);
                }

                if (invokedSymbol is IPropertySymbol propertySymbol)
                {
                    Occurrences.Add(new ReferenceOccurrence(propertySymbol.Type, new ReferenceLocation()));

                    AddDependentTypes(propertySymbol.Type);
                }

                if (invokedSymbol != null)
                {
                    ITypeSymbol? addedType = null;

                    if (invokedSymbol is ITypeSymbol type)
                    {
                        Occurrences.Add(new ReferenceOccurrence(type, new ReferenceLocation()));
                        addedType = type;
                    }

                    if (invokedSymbol.ContainingType != null)
                    {
                        Occurrences.Add(new ReferenceOccurrence(invokedSymbol.ContainingType, new ReferenceLocation()));
                        addedType = invokedSymbol.ContainingType;
                    }

                    if (addedType != null)
                    {
                        AddDependentTypes(addedType);
                    }
                }
            }
        }

        private void AddDependentTypes(ITypeSymbol addedType)
        {
            var current = addedType.BaseType;

            while (current != null)
            {
                Occurrences.Add(new ReferenceOccurrence(current, new ReferenceLocation()));
                current = current.BaseType;
            }

            foreach (var @interface in addedType.AllInterfaces)
            {
                Occurrences.Add(new ReferenceOccurrence(@interface, new ReferenceLocation()));
            }

            if (addedType is INamedTypeSymbol namedType)

                foreach (var arg in namedType.TypeArguments)
                {
                    Occurrences.Add(new ReferenceOccurrence(arg, new ReferenceLocation()));
                }
        }
    }
}
