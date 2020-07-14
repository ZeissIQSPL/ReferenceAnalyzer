namespace PropertyUsage
{
    class Test
    {
        void TestMethod()
        {
            var i = Expected.ExpectedProperty;
        }
    }

    class Expected
    {
        public static int ExpectedProperty { get; }
    }
}
