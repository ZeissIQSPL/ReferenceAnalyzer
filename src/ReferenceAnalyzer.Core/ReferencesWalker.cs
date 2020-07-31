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
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var model = _compilation.GetSemanticModel(node.SyntaxTree);

            var syntaxNodes = node.DescendantNodesAndSelf().Where(d => !d.DescendantNodes().Any());
            foreach (var d in syntaxNodes)
            {
                var invokedSymbol = model.GetSymbolInfo(d).Symbol ??
                                    model.GetSymbolInfo(d).CandidateSymbols.FirstOrDefault();


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
                        var current = addedType.BaseType;

                        while (current != null)
                        {
                            Occurrences.Add(new ReferenceOccurrence(current, new ReferenceLocation()));
                            current = current.BaseType;
                        }

                        foreach (var @interface in addedType.Interfaces)
                        {
                            Occurrences.Add(new ReferenceOccurrence(@interface, new ReferenceLocation()));
                        }
                    }
                }
            }


            base.VisitClassDeclaration(node);
        }
    }
}
