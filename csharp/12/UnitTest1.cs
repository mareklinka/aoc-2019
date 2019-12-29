using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace _12
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
            var moons = File.ReadAllLines("input.txt").Select(_ => new Moon(_)).ToArray();

            SimulateMoons(1000, moons);

            Assert.Equal(6678, moons.Sum(_ => _.TotalEnergy));
        }

        [Fact]
        public void Part2()
        {
            // whoever has the time to actually try and figure this out? per-axis independent periods
            // then, to get the total period, you take least common divisor of the per-axis periods,
            // which gives the first number that's divisible by all the independent periods - in other words, where the periods meet
            var moons = File.ReadAllLines("input.txt").Select(_ => new Moon(_)).ToArray();

            var result = CalculatePeriod(moons);

            Assert.Equal(496734501382552, result);
        }

        [Theory]
        [InlineData("<x=-1, y=0, z=2>\n<x=2, y=-10, z=-7>\n<x=4, y=-8, z=8>\n<x=3, y=5, z=-1>", 10, 179)]
        [InlineData("<x=-8, y=-10, z=0>\n<x=5, y=5, z=10>\n<x=2, y=-7, z=3>\n<x=9, y=-8, z=-3>", 100, 1940)]
        public void MovementTest(string initialState, int steps, int expected)
        {
            var moons = initialState.Split("\n").Select(_ => new Moon(_)).ToArray();

            SimulateMoons(steps, moons);

            Assert.Equal(expected, moons.Sum(_ => _.TotalEnergy));
        }

        [Theory]
        [InlineData("<x=-1, y=0, z=2>\n<x=2, y=-10, z=-7>\n<x=4, y=-8, z=8>\n<x=3, y=5, z=-1>", 2772)]
        [InlineData("<x=-8, y=-10, z=0>\n<x=5, y=5, z=10>\n<x=2, y=-7, z=3>\n<x=9, y=-8, z=-3>", 4686774924)]
        public void PeriodTest(string initialState, long expected)
        {
            var moons = initialState.Split("\n").Select(_ => new Moon(_)).ToArray();

            var result = CalculatePeriod(moons);

            Assert.Equal(expected, result);
        }

        private void SimulateMoons(int steps, Moon[] moons)
        {   
            var tuples = GetMoonTuples(moons).ToList();

            for (var step = 0; step < steps; ++step)
            {
                foreach (var (m1, m2) in tuples)
                {
                    var xGravity = ComputeGravityAlongAxis(m1.Position.X, m2.Position.X);
                    var yGravity = ComputeGravityAlongAxis(m1.Position.Y, m2.Position.Y);
                    var zGravity = ComputeGravityAlongAxis(m1.Position.Z, m2.Position.Z);

                    m1.Velocity = new P
                        { X = m1.Velocity.X + xGravity, Y = m1.Velocity.Y + yGravity, Z = m1.Velocity.Z + zGravity };
                    m2.Velocity = new P
                        { X = m2.Velocity.X - xGravity, Y = m2.Velocity.Y - yGravity, Z = m2.Velocity.Z - zGravity };
                }

                foreach (var m in moons)
                {
                    m.Move();
                }
            }
        }

        private long CalculatePeriod(Moon[] moons)
        {
            var tuples = GetMoonTuples(moons).ToList();
            var states = new HashSet<(int, int, int, int, int, int, int, int)>();
            var xPeriod = 0;
            var yPeriod = 0;
            var zPeriod = 0;

            while (true)
            {
                var state = (moons[0].Velocity.X, moons[1].Velocity.X, moons[2].Velocity.X, moons[3].Velocity.X,
                    moons[0].Position.X, moons[1].Position.X, moons[2].Position.X, moons[3].Position.X);

                if (states.Contains(state))
                {
                    break;
                }

                states.Add(state);
                foreach (var (m1, m2) in tuples)
                {
                    var xGravity = ComputeGravityAlongAxis(m1.Position.X, m2.Position.X);
                    m1.Velocity = new P
                        { X = m1.Velocity.X + xGravity, Y = m1.Velocity.Y, Z = m1.Velocity.Z };
                    m2.Velocity = new P
                        { X = m2.Velocity.X - xGravity, Y = m2.Velocity.Y, Z = m2.Velocity.Z };
                }

                foreach (var m in moons)
                {
                    m.Move();
                }

                xPeriod++;
            }

            states.Clear();

            while (true)
            {
                var state = (moons[0].Velocity.Y, moons[1].Velocity.Y, moons[2].Velocity.Y, moons[3].Velocity.Y,
                    moons[0].Position.Y, moons[1].Position.Y, moons[2].Position.Y, moons[3].Position.Y);
                
                if (states.Contains(state))
                {
                    break;
                }

                states.Add(state);
                foreach (var (m1, m2) in tuples)
                {
                    var yGravity = ComputeGravityAlongAxis(m1.Position.Y, m2.Position.Y);
                    m1.Velocity = new P
                        { X = m1.Velocity.X, Y = m1.Velocity.Y + yGravity, Z = m1.Velocity.Z };
                    m2.Velocity = new P
                        { X = m2.Velocity.X, Y = m2.Velocity.Y - yGravity, Z = m2.Velocity.Z };
                }

                foreach (var m in moons)
                {
                    m.Move();
                }

                yPeriod++;
            }

            states.Clear();

            while (true)
            {
                var state = (moons[0].Velocity.Z, moons[1].Velocity.Z, moons[2].Velocity.Z, moons[3].Velocity.Z,
                    moons[0].Position.Z, moons[1].Position.Z, moons[2].Position.Z, moons[3].Position.Z);
                
                if (states.Contains(state))
                {
                    break;
                }

                states.Add(state);
                foreach (var (m1, m2) in tuples)
                {
                    var zGravity = ComputeGravityAlongAxis(m1.Position.Z, m2.Position.Z);
                    m1.Velocity = new P
                        { X = m1.Velocity.X, Y = m1.Velocity.Y, Z = m1.Velocity.Z + zGravity };
                    m2.Velocity = new P
                        { X = m2.Velocity.X, Y = m2.Velocity.Y, Z = m2.Velocity.Z - zGravity };
                }

                foreach (var m in moons)
                {
                    m.Move();
                }

                zPeriod++;
            }

            return GetLCM(xPeriod, GetLCM(yPeriod, zPeriod));
        }

        static long GetLCM(long a, long b)
        {
            return (a * b) / GetGCD(a, b);
        }

        static long GetGCD(long a, long b)
        {
            while (a != b)
                if (a < b) b = b - a;
                else a = a - b;
            return (a);
        }

        private static int ComputeGravityAlongAxis(int v1, int v2)
        {
            if (v1 < v2)
                return 1;
            else if (v1 == v2)
                return 0;
            else
                return -1;
        }

        private IEnumerable<(Moon, Moon)> GetMoonTuples(Moon[] moons)
        {
            for (var i = 0; i < moons.Length; i++)
            {
                var m1 = moons[i];
                foreach (var m2 in moons.Skip(i + 1))
                {
                    yield return (m1, m2);
                }
            }
        }

        private class Moon
        {
            public Moon(string position)
            {
                var coordinates = position[1..^1].Split(", ").Select(_ => int.Parse(_[2..])).ToArray();
                Position = new P { X = coordinates[0], Y = coordinates[1], Z = coordinates[2] };
                Velocity = new P();
            }

            public P Position { get; set; }

            public P Velocity { get; set; }

            public void Move()
            {
                Position = new P
                {
                    X = Position.X + Velocity.X,
                    Y = Position.Y + Velocity.Y,
                    Z = Position.Z + Velocity.Z,
                };
            }

            public int PotentialEnergy => Math.Abs(Position.X) + Math.Abs(Position.Y) + Math.Abs(Position.Z);

            public int KineticEnergy => Math.Abs(Velocity.X) + Math.Abs(Velocity.Y) + Math.Abs(Velocity.Z);

            public int TotalEnergy => PotentialEnergy * KineticEnergy;
        }

        [DebuggerDisplay("{X}, {Y}, {Z}")]
        private class P : IEquatable<P>
        {
            public int X { get; set; }

            public int Y { get; set; }

            public int Z { get; set; }

            public bool Equals(P other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return X == other.X && Y == other.Y && Z == other.Z;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((P) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y, Z);
            }
        }
    }
}
