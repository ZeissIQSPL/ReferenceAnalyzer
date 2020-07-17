using System;

namespace Attribute
{
    [Expected]
    class Test
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    class Expected : Attribute
    {
    }
}
