using System;
using Xunit;

namespace Tringo.WebApp.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Sample_Test()
        {
            // arrange
            var sum = 1;
            // act
            sum++;
            // assert
            Assert.Equal(2, sum);
        }
    }
}
