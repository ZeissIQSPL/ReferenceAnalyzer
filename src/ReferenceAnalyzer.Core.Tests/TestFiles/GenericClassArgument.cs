namespace GenericClassArgument
{
    class Test
    {
        public void TestMethod()
        {
            var t = new Test2<Expected>();
        }
    }

    class Test2<T>
    {

    }

    class Expected
    {

    }
}
