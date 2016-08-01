using NUnit.Framework;
using Shouldly;

namespace KitchenSink.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void Test1()
        {
            true.ShouldBe(true);
        }
    }
}
