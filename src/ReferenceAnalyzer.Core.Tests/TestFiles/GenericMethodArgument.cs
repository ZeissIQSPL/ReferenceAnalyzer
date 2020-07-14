namespace GenericMethodArgument
{
    class Test
    {
        void TestMethodInvocation()
        {
            TestMethod<Expected>(null);
        }

        void TestMethod<T>(T arg) {}
    }

    class Expected
    {

    }
}
