using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Xunit;

namespace _16
{
    public class UnitTest1
    {
        [Fact]
        public void Benchmark()
        {
            BenchmarkRunner.Run(GetType());
        }

        [Fact]
        public void Part1()
        {
            var input = File.ReadAllText("input.txt").Trim().ToCharArray().Select(_ => int.Parse(_.ToString())).ToList();

            for (var i = 1; i <= 100; ++i)
            {
                input = Phase(input, new List<int> { 0, 1, 0, -1 });
            }

            Assert.Equal("73127523", string.Join(string.Empty, input).Substring(0, 8));
        }

        [Fact]
        public void Part2()
        {
            var text = File.ReadAllText("input.txt");
            var input = text.Trim().ToCharArray().Select(_ => int.Parse(_.ToString())).ToArray();
            var offset = int.Parse(text.Substring(0, 7));
            input = Repeat10K(input).Skip(offset).ToArray();

            for (var i = 1; i <= 100; ++i)
            {
                input = PhaseBack(input);
            }

            Assert.Equal("80284420", string.Join(string.Empty, input.Take(8)));
        }

        [Theory]
        [InlineData("12345678", 4, "01029498")]
        [InlineData("80871224585914546619083218645595", 100, "24176176")]
        [InlineData("19617804207202209144916044189917", 100, "73745418")]
        [InlineData("69317163492948606335995924319873", 100, "52432133")]
        public void TestPart1(string inputString, int phases, string expected)
        {
            var input = inputString.ToCharArray().Select(_ => int.Parse(_.ToString())).ToList();

            for (var i = 1; i <= phases; ++i)
            {
                input = Phase(input, new List<int> { 0, 1, 0, -1 });
            }

            Assert.Equal(expected, string.Join(string.Empty, input).Substring(0, 8));
        }

        [Theory]
        [InlineData("03036732577212944063491565474664", 100, "84462026")]
        [InlineData("02935109699940807407585447034323", 100, "78725270")]
        [InlineData("03081770884921959731165446850517", 100, "53553731")]
        public void TestPart2(string inputString, int phases, string expected)
        {
            var input = inputString.ToCharArray().Select(_ => int.Parse(_.ToString())).ToArray();
            var offset = int.Parse(inputString.Substring(0, 7));
            input = Repeat10K(input).Skip(offset).ToArray();

            for (var i = 1; i <= phases; ++i)
            {
                input = PhaseBack(input);
            }

            Assert.Equal(expected, string.Join(string.Empty, input.Take(8)));
        }

        public int[] Input { get; set; }

        public int Offset { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var text = File.ReadAllText("input.txt");
            Input = text.Trim().ToCharArray().Select(_ => int.Parse(_.ToString())).ToArray();
            Offset = int.Parse(text.Substring(0, 7));
            Input = Repeat10K(Input).Skip(Offset).ToArray();
        }

        [Benchmark(Baseline = true)]
        public void Part2Benchmark()
        {
            var input = Input;
            for (var i = 1; i <= 100; ++i)
            {
                input = PhaseBack(input);
            }
        }

        [Benchmark]
        public void Part2VectorBenchmark()
        {
            var input = Input;
            for (var i = 1; i <= 100; ++i)
            {
                input = PhaseBackVector(input);
            }

            Assert.Equal("80284420", string.Join(string.Empty, input.Take(8)));
        }

        private static int[] PhaseBack(int[] input)
        {
            // the trick here is that we offset is in the second half of the input
            // for the elements in the second half, the next value is only dependent on the elements in the second half again
            // if you look at the matrix calculation example, the first element in the second half is dependent on all elements in the second half
            // the half+1 element is only dependent on sum of half+1 to the end
            // the half+2 element is dependent on sum of half+2 to the end etc (it's an upper triangular matrix with only 1s)
            // therefore we do the sum from the offset+1 to the end, that's what offset+1 element depends on
            // then subtract the offset+1 element, that's what the offset+2 element depends on etc.
            // O(N) for N = TotalLength - offset

            var list = new int[input.Length];

            var sum = input.Sum();

            for (var index = 0; index < input.Length; index++)
            {
                var i = input[index];
                list[index] = Math.Abs(sum) % 10;
                sum -= i;
            }

            return list;
        }

        private static int[] PhaseBackVector(int[] input)
        {
            // the trick here is that we offset is in the second half of the input
            // for the elements in the second half, the next value is only dependent on the elements in the second half again
            // if you look at the matrix calculation example, the first element in the second half is dependent on all elements in the second half
            // the half+1 element is only dependent on sum of half+1 to the end
            // the half+2 element is dependent on sum of half+2 to the end etc (it's an upper triangular matrix with only 1s)
            // therefore we do the sum from the offset+1 to the end, that's what offset+1 element depends on
            // then subtract the offset+1 element, that's what the offset+2 element depends on etc.
            // O(N) for N = TotalLength - offset

            var list = new int[input.Length];

            var sumVector = new Vector<int>(0);

            var subVectors = input.Length / Vector<int>.Count;

            for (var i = 0; i < subVectors; ++i)
            {
                sumVector += new Vector<int>(input.AsSpan(Vector<int>.Count * i));
            }

            var remainder = input.Skip(Vector<int>.Count * subVectors).Sum();

            var sum = Vector.Dot(sumVector, Vector<int>.One) + remainder;

            for (var index = 0; index < input.Length; index++)
            {
                var i = input[index];
                list[index] = Math.Abs(sum) % 10;
                sum -= i;
            }

            return list;
        }

        private static List<int> Phase(IReadOnlyCollection<int> input, IReadOnlyCollection<int> pattern)
        {
            var list = new List<int>();

            for (var index = 1; index <= input.Count; index++)
            {
                var sum = input.Zip(GetPatternForPhase(pattern, index).Skip(1))
                    .Select(_ => (_.First * _.Second)).Sum();

                list.Add(Math.Abs(sum) % 10);
            }

            return list;
        }

        private static IEnumerable<int> Repeat10K(IReadOnlyCollection<int> input)
        {
            for (var i = 0; i < 10000; ++i)
            {
                foreach (var i1 in input)
                {
                    yield return i1;
                }
            }
        }

        private static IEnumerable<int> GetPatternForPhase(IReadOnlyCollection<int> patternSource, int phaseNumber)
        {
            while (true)
            {
                foreach (var i in patternSource)
                {
                    for (var j = 0; j < phaseNumber; ++j)
                    {
                        yield return i;
                    }
                }
            }
        }
    }
}
