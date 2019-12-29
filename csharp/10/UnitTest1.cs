using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace _10
{
    public class UnitTest1
    {
        [Fact]
        public void Part1()
        {
            var asteroids = new HashSet<P>();

            var lines = File.ReadAllLines("input.txt");

            for (var row = 0; row < lines.Length; row++)
            {
                for (var col = 0; col < lines[0].Length; col++)
                {
                    if (lines[row][col] == '#')
                    {
                        asteroids.Add(new P { X = col, Y = row });
                    }
                }
            }

            var (_, count) = SelectObservationPoint(asteroids, lines);

            Assert.Equal(227, count);
        }

        [Fact]
        public void Part2()
        {
            var asteroids = new HashSet<P>();

            var lines = File.ReadAllLines("input.txt");

            for (var row = 0; row < lines.Length; row++)
            {
                for (var col = 0; col < lines[0].Length; col++)
                {
                    if (lines[row][col] == '#')
                    {
                        asteroids.Add(new P { X = col, Y = row });
                    }
                }
            }

            var (position, _) = SelectObservationPoint(asteroids, lines);

            var (_, angles) = CountVisible(asteroids, position, lines[0].Length, lines.Length);

            var vaporizationCount = 0;

            var orderedViewingAngles = angles.Keys.OrderBy(_ => _).ToList(); // we need to start moving in strict clockwise direction

            var index = orderedViewingAngles.IndexOf(270); // straight north

            for (var i = 0; i < orderedViewingAngles.Count; ++i)
            {
                var angle = orderedViewingAngles[(index + i) % orderedViewingAngles.Count];
                var pointVisibleUnderAngle = angles[angle];

                if (++vaporizationCount == 200)
                {
                    var sum = pointVisibleUnderAngle.X * 100 + pointVisibleUnderAngle.Y;
                    Assert.Equal(604, sum);
                    return;
                }
            }

            Assert.True(false);
        }

        [Fact]
        public void TestSmall1()
        {
            var asteroids = new HashSet<P>();

            var lines = File.ReadAllLines("input_small.txt");

            for (var row = 0; row < lines.Length; row++)
            {
                for (var col = 0; col < lines[0].Length; col++)
                {
                    if (lines[row][col] == '#')
                    {
                        asteroids.Add(new P { X = col, Y = row });
                    }
                }
            }

            var (position, count) = SelectObservationPoint(asteroids, lines);

            Assert.Equal(new P { X = 11, Y = 13 }, position);
            Assert.Equal(210, count);
        }

        private (P Position, int Count) SelectObservationPoint(HashSet<P> asteroids, string[] lines)
        {
            var bestVisible = 0;
            P bestPosition = null;

            foreach (var a in asteroids)
            {
                var (count, _) = CountVisible(asteroids, a, lines[0].Length, lines.Length);

                if (count > bestVisible)
                {
                    bestVisible = count;
                    bestPosition = a;
                }
            }

            return (bestPosition, bestVisible);
        }


        [Theory]
        [InlineData("#.\n#.", 0, 0, 1)]
        [InlineData("##\n##", 0, 0, 3)]
        [InlineData("#.#\n..#\n###", 0, 0, 5)]
        [InlineData("###\n#.#\n###", 1, 1, 8)]
        [InlineData("####\n####\n####\n####", 1, 1, 12)]
        [InlineData("....\n####\n....\n####", 1, 1, 6)]
        public void TestVisibility(string field, int x, int y, int expected)
        {
            var asteroids = new HashSet<P>();
            var lines = field.Split("\n");

            for (var row = 0; row < lines.Length; row++)
            {
                for (var col = 0; col < lines[0].Length; col++)
                {
                    if (lines[row][col] == '#')
                    {
                        asteroids.Add(new P { X = col, Y = row });
                    }
                }
            }

            var (count, _) = CountVisible(asteroids, new P { X = x, Y = y }, lines[0].Length, lines.Length);

            Assert.Equal(expected, count);
        }

        private static (int count, Dictionary<double, P> ViewingAngles) CountVisible(HashSet<P> field, P asteroid, int width, int height)
        {
            var q = new Queue<P>(); // depth-first search to always find the closest viewable asteroid first
            var visited = new HashSet<P> {asteroid};
            var viewingAngles = new Dictionary<double, P>(); // asteroid is hidden by another if they share a viewing angle, the value here indicates the first VISIBLE asteroid

            foreach (var p in asteroid.Around())
            {
                q.Enqueue(p);
            }

            while (q.Any())
            {
                var target = q.Dequeue();

                if (target.X < 0 || target.Y < 0 || target.X >= width || target.Y >= height)
                {
                    continue;
                }

                if (visited.Contains(target))
                {
                    continue;
                }

                if (!field.Contains(target))
                {
                    visited.Add(target);
                    foreach (var p in target.Around().Where(p => !visited.Contains(p)))
                    {
                        q.Enqueue(p);
                    }

                    continue;
                }

                var angle = Math.Atan2(target.Y - asteroid.Y, target.X - asteroid.X) * 180 / Math.PI;
                angle = angle < 0 ? 360 - Math.Abs(angle) : angle; // translate to 0 - 360 deg monotonic in clockwise direction

                if (!viewingAngles.ContainsKey(angle))
                {
                    viewingAngles.Add(angle, target);
                }

                visited.Add(target);

                foreach (var p in target.Around().Where(p => !visited.Contains(p)))
                {
                    q.Enqueue(p);
                }
            }

            return (viewingAngles.Count, viewingAngles);
        }

        private class P : IEquatable<P>
        {
            public int X { get; set; }

            public int Y { get; set; }

            public bool Equals(P other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return X == other.X && Y == other.Y;
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
                unchecked
                {
                    return (X * 397) ^ Y;
                }
            }

            public IEnumerable<P> Around()
            {
                yield return new P { X = X - 1, Y = Y - 1 };
                yield return new P { X = X, Y = Y - 1 };
                yield return new P { X = X + 1, Y = Y - 1 };

                yield return new P { X = X - 1, Y = Y };
                yield return new P { X = X + 1, Y = Y };

                yield return new P { X = X - 1, Y = Y + 1 };
                yield return new P { X = X, Y = Y + 1 };
                yield return new P { X = X + 1, Y = Y + 1 };
            }
        }
    }
}
