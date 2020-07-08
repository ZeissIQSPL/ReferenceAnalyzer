using System;
using Project2;
using Xunit;

namespace Project1
{
    public class Class1
    {
        public void TestMethod()
        {
            ReferencedClass.ReferencedMethod();

            Assert.True(true);
        }
    }
}
