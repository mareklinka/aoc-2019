using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace _01
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Part1()
        {
            var numbers = File.ReadAllLines("input.txt").Select(int.Parse).Select(_ => _ / 3 - 2).Sum();

            output.WriteLine($"Weight: {numbers}");
        }

        [Fact]
        public void Part2()
        {
            var numbers = File.ReadAllLines("input.txt").Select(int.Parse).Select(_ => GetFuelFor(_)).Sum();

            output.WriteLine($"Weight: {numbers}");
        }

        [Theory]
        [InlineData(14, 2)]
        [InlineData(1969, 966)]
        [InlineData(100756, 50346)]
        public void TestRecursion(int weight, int expected)
        {
            Assert.Equal(expected, GetFuelFor(weight));
        }

        private int GetFuelFor(int weight)
        {
            var fuel = weight / 3 - 2;
            var accumulator = 0;

            while (fuel > 0)
            {
                accumulator += fuel;
                fuel = fuel / 3 - 2;
            }

            return accumulator;
        }
    }
}
