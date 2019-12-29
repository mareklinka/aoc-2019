using System;
using Xunit;
using Xunit.Abstractions;

namespace _04
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Part1()
        {
            var count = 0;
            for (var i = 272091; i <= 815432; i++)
            {
                if (Evaluate1(i))
                {
                    _output.WriteLine(i.ToString());
                    ++count;
                }
            }

            _output.WriteLine(count.ToString());
        }

        [Fact]
        public void Part2()
        {
            var count = 0;
            for (var i = 272091; i <= 815432; i++)
            {
                if (Evaluate2(i))
                {
                    _output.WriteLine(i.ToString());
                    ++count;
                }
            }

            _output.WriteLine(count.ToString());
        }

        [Theory]
        [InlineData(111111, true)]
        [InlineData(223450, false)]
        [InlineData(123789, false)]
        public void EvaluateTest(int number, bool expected)
        {
            Assert.Equal(expected, Evaluate1(number));
        }

        private static bool Evaluate1(int number)
        {
            var previous = -1;
            var hasPair = false;

            for (var i = 5; i >= 0; --i)
            {
                var digit = number / ((int)Math.Pow(10, i));

                if (digit < previous)
                {
                    return false;
                }

                if (previous == digit)
                {
                    hasPair = true;
                }

                number -= digit * (int)Math.Pow(10, i);
                previous = digit;
            }

            return hasPair;
        }

        [Theory]
        [InlineData(112233, true)]
        [InlineData(123444, false)]
        [InlineData(111122, true)]
        public void Evaluate2Test(int number, bool expected)
        {
            Assert.Equal(expected, Evaluate2(number));
        }

        private static bool Evaluate2(int number)
        {
            var previous = -1;
            var runLength = 1;
            var hasExactPair = false;

            for (var i = 5; i >= 0; --i)
            {
                var digit = number / ((int)Math.Pow(10, i));

                if (digit < previous)
                {
                    return false;
                }

                if (hasExactPair)
                {
                    runLength = 1;
                }
                else if (previous == digit)
                {
                    ++runLength;
                }
                else
                {
                    hasExactPair = runLength == 2;
                    runLength = 1;
                }

                number -= digit * (int)Math.Pow(10, i);
                previous = digit;
            }

            return hasExactPair || runLength == 2;
        }
    }
}
