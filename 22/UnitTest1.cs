using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Xunit;

namespace _22
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var lines = File.ReadAllLines("input.txt");

            var deck = Enumerable.Range(0, 10007).ToArray();

            foreach (var line in lines)
            {
                switch (line)
                {
                    case var s when s.StartsWith("deal with increment"):
                        deck = Increment(deck, int.Parse(s.Split(" ").Last()));
                        break;
                    case var s when s.StartsWith("cut"):
                        deck = Cut(deck, int.Parse(s.Split(" ").Last()));
                        break;
                    case var s when s.StartsWith("deal into new"):
                        deck = DealNew(deck);
                        break;
                }
            }

            Assert.Equal(4284, Array.IndexOf(deck, 2019));
        }

        [Fact]
        public void Test2()
        {
            var lines = File.ReadAllLines("input.txt").Reverse().ToArray();
            const long deckSize = 119315717514047L;

            BigInteger a = 1;
            BigInteger b = 0;

            foreach (var line in lines)
            {
                switch (line)
                {
                    case var s when s.StartsWith("deal with increment"):
                        var param = int.Parse(s.Split(" ").Last());

                        // modInverse for prime base https://en.wikipedia.org/wiki/Modular_multiplicative_inverse
                        var inversePower = BigInteger.ModPow(param,  deckSize - 2,deckSize);
                        a = a * inversePower % deckSize;
                        b = b * inversePower % deckSize;

                        break;
                    case var s when s.StartsWith("cut"):
                        // cutting just shifts the B parameter - the offset
                        var i = long.Parse(s.Split(" ").Last());

                        if (i < 0)
                        {
                            i += deckSize;
                        }
                        b += i;
                        break;
                    case var s when s.StartsWith("deal into new"):
                        // reverse changes sign (offset changes by one before sign inverse)
                        a = -a;
                        b = -++b;
                        break;
                }

                a %= deckSize;
                b %= deckSize;
                if (b < 0)
                    b += deckSize;
                if (a < 0)
                    a += deckSize;
            }

            const long N = 101741582076661;

            // we are looking for an N-th power of the equation describing the function composition:
            // (a*x + b)^N
            // ax + b
            // a^2x + ab + b
            // a^3x + a^2b + ab + b
            // a^4x + a^3b + a^2b + ab + b etc.
            // in general, the nth term looks like
            // a^nx + a^(n - 1)b + a^(n - 2)b + ... + a^2b + ab + b
            // which can be factored into
            // a^nx + b(1 - a^n) / (1 - a)

            var part1 = 2020 * BigInteger.ModPow(a, N, deckSize);
            var part2 = b * (BigInteger.ModPow(a, N, deckSize) - 1);
            var part3 = BigInteger.ModPow(a - 1, deckSize - 2, deckSize);

            var ans = (part1 + part2 * part3) % deckSize;
            Assert.Equal(96797432275571, (long)ans);
        }

        private static int[] DealNew(int[] deck) => deck.Reverse().ToArray();

        private static int[] Cut(int[] deck, int n)
        {
            if (n < 0)
            {
                return deck[(deck.Length + n)..].Concat(deck[..(deck.Length + n)]).ToArray();
            }
            else
            {
                return deck[n..].Concat(deck[..n]).ToArray();
            }
        }

        private static int[] Increment(int[] deck, int n)
        {
            var result = new int[deck.Length];

            for (var i = 0; i < deck.Length; ++i)
            {
                result[(i * n) % deck.Length] = deck[i];
            }

            return result;
        }

        [Theory]
        [InlineData("0 1 2 3 4 5 6 7 8 9", 3, "3 4 5 6 7 8 9 0 1 2")]
        [InlineData("0 1 2 3 4 5 6 7 8 9", -4, "6 7 8 9 0 1 2 3 4 5")]
        public void TestCutting(string source, int n, string expected)
        {
            var a = source.Split(" ").Select(int.Parse).ToArray();
            Assert.Equal(expected, string.Join(" ", Cut(a, n)));
        }

        [Theory]
        [InlineData("0 1 2 3 4 5 6 7 8 9", 3, "0 7 4 1 8 5 2 9 6 3")]
        public void TestIncrement(string source, int n, string expected)
        {
            var a = source.Split(" ").Select(int.Parse).ToArray();
            Assert.Equal(expected, string.Join(" ", Increment(a, n)));
        }
    }
}
