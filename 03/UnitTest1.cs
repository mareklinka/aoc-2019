using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace _03
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
            var lines = File.ReadAllLines("input.txt");
            var w1 = lines[0];
            var w2 = lines[1];

            _output.WriteLine(ClosestCrossingToOrigin(w1, w2).ToString());
        }

        [Fact]
        public void Part2()
        {
            var lines = File.ReadAllLines("input.txt");
            var w1 = lines[0];
            var w2 = lines[1];

            _output.WriteLine(ClosestCrossingTravelTime(w1, w2).ToString());
        }

        [Theory]
        [InlineData("R8,U5,L5,D3", "U7,R6,D4,L4", 6)]
        [InlineData("R75,D30,R83,U83,L12,D49,R71,U7,L72", "U62,R66,U55,R34,D71,R55,D58,R83", 159)]
        [InlineData("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51", "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7", 135)]
        public void TestCrossing(string w1, string w2, int expected)
        {
            var c = ClosestCrossingToOrigin(w1, w2);

            Assert.Equal(expected, c);
        }

        [Theory]
        [InlineData("R8,U5,L5,D3", "U7,R6,D4,L4", 30)]
        [InlineData("R75,D30,R83,U83,L12,D49,R71,U7,L72", "U62,R66,U55,R34,D71,R55,D58,R83", 610)]
        [InlineData("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51", "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7", 410)]
        public void TestCrossingDistance(string w1, string w2, int expected)
        {
            var c = ClosestCrossingTravelTime(w1, w2);

            Assert.Equal(expected, c);
        }

        private int ClosestCrossingTravelTime(string wireString1, string wireString2)
        {
            var wire1 = GetPointsForWire(wireString1);
            var wire2 = GetPointsForWire(wireString2);

            var hs1 = new HashSet<P>(wire1);
            var hs2 = new HashSet<P>(wire2);

            hs2.IntersectWith(hs1);

            var min = hs2.Select(_ => DistanceTo(_, wire1.ToArray()) + DistanceTo(_, wire2.ToArray())).ToList();



            return min.Min();
        }

        private int DistanceTo(P crossing, P[] wire)
        {
            return Array.IndexOf(wire, crossing) + 1;
        }

        private int ClosestCrossingToOrigin(string wireString1, string wireString2)
        {
            var wire1 = new HashSet<P>(GetPointsForWire(wireString1));
            var wire2 = new HashSet<P>(GetPointsForWire(wireString2));

            wire2.IntersectWith(wire1);

            var closestSum = wire2.Select(_ => Math.Abs(_.Y) + Math.Abs(_.X)).Min();

            return closestSum;
        }

        private List<P> GetPointsForWire(string wireString)
        {
            var list = new List<P>();

            var parts = wireString.Split(",");

            var position = new P(0, 0);

            foreach (var part in parts)
            {
                switch (part[0])
                {
                    case 'R':
                        for (var i = 1; i <= int.Parse(part.Substring(1)); ++i)
                        {
                            list.Add(new P(position.X + i, position.Y));
                        }

                        position = new P(position.X + int.Parse(part.Substring(1)), position.Y);
                        break;
                    case 'L':
                        for (var i = 1; i <= int.Parse(part.Substring(1)); ++i)
                        {
                            list.Add(new P(position.X - i, position.Y));
                        }

                        position = new P(position.X - int.Parse(part.Substring(1)), position.Y);
                        break;
                    case 'U':
                        for (var i = 1; i <= int.Parse(part.Substring(1)); ++i)
                        {
                            list.Add(new P(position.X, position.Y - i));
                        }

                        position = new P(position.X, position.Y - int.Parse(part.Substring(1)));
                        break;
                    case 'D':
                        for (var i = 1; i <= int.Parse(part.Substring(1)); ++i)
                        {
                            list.Add(new P(position.X, position.Y + i));
                        }

                        position = new P(position.X, position.Y + int.Parse(part.Substring(1)));
                        break;
                    default:
                        throw new Exception();
                }
            }

            return list;
        }

        private struct P
        {
            public P(int x, int y)
            {
                X = x;
                Y = y;
            }

            public readonly int X;

            public readonly int Y;

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }

            public override bool Equals(object obj)
            {
                return obj switch
                {
                    P p => p.X == X && p.Y == Y,
                    _ => false
                };
            }
        }
    }
}
