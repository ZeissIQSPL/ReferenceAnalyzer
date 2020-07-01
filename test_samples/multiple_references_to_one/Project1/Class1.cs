using System;
using Project2;

namespace Project1
{
    public class Class1
    {
        public void TestMethod() {
            var a = new ReferencedType();

            a.ReferencedMethod();

            var b = a.ReferencedProperty;

            a.ReferencedProperty2 = b;
        }
    }
}
