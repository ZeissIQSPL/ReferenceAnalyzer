using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Moq;

namespace ReferenceExplorer.Tests
{

    public class RoslynVisitorTests
    {
        private readonly ICollection<Mock<INamespaceOrTypeSymbol>> _Symbols = new[]
        {
            new Mock<INamespaceOrTypeSymbol>(),new Mock<INamespaceOrTypeSymbol>()
        };

        private readonly Mock<INamespaceSymbol> _NamespaceSymbol = new Mock<INamespaceSymbol>();


        [Fact]
        public void AfterVisitingNamespaceASymbolIsVisited()
        {

            _NamespaceSymbol.Setup(x => x.GetMembers()).Returns(new[] { _Symbols.First().Object });

            var sut = new RoslynVisitor(null);

            sut.VisitNamespace(_NamespaceSymbol.Object);

            _Symbols.First().Verify(x => x.Accept(sut));

        }

        [Fact]
        public void AfterVisitingNamespaceAllMemberSymbolsAreVisited()
        {
            _NamespaceSymbol.Setup(x => x.GetMembers()).Returns(_Symbols.Select(x => x.Object));

            var sut = new RoslynVisitor(null);

            sut.VisitNamespace(_NamespaceSymbol.Object);

            foreach (var symbol in _Symbols)
            {
                symbol.Verify(x => x.Accept(sut));
            }
        }


    }
}
