namespace MethodInvocation
{
    class Test
    {
        void TestMethod()
        {
            Expected.ExpectedMethod();
        }
    }

    class Expected
    {
        public static void ExpectedMethod() {}
    }
}
