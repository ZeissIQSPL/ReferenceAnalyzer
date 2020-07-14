namespace ConstructorInvocation
{
    class Test
    {
        void TestMethod()
        {
            var e = new Expected();
        }
    }

    class Expected
    {
        public Expected()
        {
        }
    }
}
