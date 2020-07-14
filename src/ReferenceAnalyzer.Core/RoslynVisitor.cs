using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReferenceAnalyzer.Core
{
    public class RoslynVisitor : SymbolVisitor
    {
        private readonly Compilation _compilation;
        private readonly IDictionary<ITypeSymbol, SyntaxTree> _syntaxTrees;

        public RoslynVisitor(Compilation compilation)
        {
            _compilation = compilation;
            Occurrences = new ConcurrentBag<ReferenceOccurrence>();
            _syntaxTrees = new ConcurrentDictionary<ITypeSymbol, SyntaxTree>();
        }

        public ConcurrentBag<ReferenceOccurrence> Occurrences { get; set; }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var s in symbol.GetMembers())
                s.Accept(this);

            base.VisitNamespace(symbol);
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            foreach (var s in symbol.GetMembers())
                s.Accept(this);

            foreach (var type in symbol.Interfaces)
                Occurrences.Add(new ReferenceOccurrence(type, new ReferenceLocation()));

            foreach (var ctor in symbol.Constructors)
                ctor.Accept(this);

            base.VisitNamedType(symbol);
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            foreach (var parameter in symbol.Parameters)
                parameter.Accept(this);

            Occurrences.Add(new ReferenceOccurrence(symbol.ReturnType, new ReferenceLocation()));
            if (!symbol.DeclaringSyntaxReferences.IsEmpty)
            {
                if (!_syntaxTrees.TryGetValue(symbol.ContainingType, out var syntaxTree))
                {
                    syntaxTree = symbol.DeclaringSyntaxReferences.First().SyntaxTree;
                    _syntaxTrees.Add(symbol.ContainingType, syntaxTree);
                }

                var nodes = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.ValueText == symbol.Name)
                    ?.DescendantNodes() ?? Enumerable.Empty<SyntaxNode>();

                var objectCreationNodes = nodes.OfType<ObjectCreationExpressionSyntax>();
                var memberAccessNodes = nodes.OfType<MemberAccessExpressionSyntax>();
                var genericNameNodes = nodes.OfType<GenericNameSyntax>();

                TraverseNodes(syntaxTree, objectCreationNodes);
                TraverseNodes(syntaxTree, memberAccessNodes);

                foreach (var n in genericNameNodes)
                    TraverseNodes(syntaxTree, n.TypeArgumentList.Arguments);
            }


            base.VisitMethod(symbol);
        }

        private void TraverseNodes(SyntaxTree syntaxTree, IEnumerable<SyntaxNode> nodes)
        {
            foreach (var invocation in nodes)
            {
                var model = _compilation.GetSemanticModel(syntaxTree);
                var invokedSymbol = model.GetSymbolInfo(invocation).Symbol ??
                                    model.GetSymbolInfo(invocation).CandidateSymbols.FirstOrDefault();


                if (invokedSymbol != null)
                {
                    if (invokedSymbol is ITypeSymbol type)
                        Occurrences.Add(new ReferenceOccurrence(type, new ReferenceLocation()));
                    if (invokedSymbol.ContainingType != null)
                        Occurrences.Add(new ReferenceOccurrence(invokedSymbol.ContainingType, new ReferenceLocation()));
                }
            }
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            Occurrences.Add(new ReferenceOccurrence(symbol.Type, new ReferenceLocation()));
            base.VisitParameter(symbol);
        }
    }
}
