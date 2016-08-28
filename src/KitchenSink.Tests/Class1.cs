using Xunit;
using Shouldly;

namespace KitchenSink.Tests
{
    public class Class1
    {
        [Fact]
        public void PassingTest1()
        {
            true.ShouldBe(true);
        }
    }
}
