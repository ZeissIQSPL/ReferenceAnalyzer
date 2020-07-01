using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReferenceExplorer
{
	public class RoslynVisitor : SymbolVisitor
	{
		private Compilation _Compilation;
        public ConcurrentBag<ITypeSymbol> UsedTypeSymbols { get; set; }
        //public HashSet<INamedTypeSymbol> UsedTypeSymbols { get; set; }
        private IDictionary<ITypeSymbol, SyntaxTree> _SyntaxTrees;
        public RoslynVisitor(Compilation compilation)
        {
            _Compilation = compilation;
            UsedTypeSymbols = new ConcurrentBag<ITypeSymbol>();
            _SyntaxTrees = new ConcurrentDictionary<ITypeSymbol, SyntaxTree>();
            
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var s in symbol.GetMembers())
            {
                s.Accept(this);
            }
            //Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            base.VisitNamespace(symbol);
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            foreach (var s in symbol.GetMembers())
            {
                s.Accept(this);
            }
	        //Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));

            foreach (var type in symbol.Interfaces)
            {
	            UsedTypeSymbols.Add(type);
            }

            foreach (var ctor in symbol.Constructors)
            {
	            ctor.Accept(this);
            }

            base.VisitNamedType(symbol);
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
	        foreach (var parameter in symbol.Parameters)
	        {
		        parameter.Accept(this);
	        }

            UsedTypeSymbols.Add(symbol.ReturnType);
	        if (!symbol.DeclaringSyntaxReferences.IsEmpty)
            {
                if (!_SyntaxTrees.TryGetValue(symbol.ContainingType, out var syntaxTree))
                {
                    syntaxTree = symbol.DeclaringSyntaxReferences.First().SyntaxTree;
                    _SyntaxTrees.Add(symbol.ContainingType, syntaxTree);
                }

                var nodes = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.ValueText == symbol.Name)?
                    .DescendantNodes() ?? Enumerable.Empty<SyntaxNode>();

                var objectCreationNodes = nodes.OfType<ObjectCreationExpressionSyntax>();
                var memberAccessNodes = nodes.OfType<MemberAccessExpressionSyntax>();
                var genericNameNodes = nodes.OfType<GenericNameSyntax>();

                TraverseNodes(syntaxTree, objectCreationNodes);
                TraverseNodes(syntaxTree, memberAccessNodes);

                foreach (var n in genericNameNodes)
                {
                    TraverseNodes(syntaxTree, n.TypeArgumentList.Arguments);
                }

            }



            base.VisitMethod(symbol);
        }

        private void TraverseNodes(SyntaxTree syntaxTree, IEnumerable<SyntaxNode> nodes)
        {
            foreach (var invocation in nodes)
            {
                var model = _Compilation.GetSemanticModel(syntaxTree);
                var invokedSymbol = model.GetSymbolInfo(invocation).Symbol;

                if (invokedSymbol == null)
                {
                    invokedSymbol = model.GetSymbolInfo(invocation).CandidateSymbols.FirstOrDefault();
                } 


                if (invokedSymbol != null)
                {
                    if (invokedSymbol is ITypeSymbol type)
                    {
                        UsedTypeSymbols.Add(type);
                    }
                    if (invokedSymbol.ContainingType != null)
                    {
                        UsedTypeSymbols.Add(invokedSymbol.ContainingType);
                    }
                }
            }
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            UsedTypeSymbols.Add(symbol.Type);
	        base.VisitParameter(symbol);
        }
	}
}
