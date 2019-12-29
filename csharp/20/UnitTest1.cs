using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace _20
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var lines = File.ReadAllLines("input.txt");

            var maze = ParseMaze(lines);
            var (portals, start, end) = ConnectPortals(maze);

            var q = new Queue<(P, int)>();
            var visited = new HashSet<P>();

            q.Enqueue((start, 0));
            var shortest = 0;

            while (q.Any())
            {
                var (position, distance) = q.Dequeue();

                if (position.Equals(end))
                {
                    shortest = distance;
                    break;
                }

                if (visited.Contains(position))
                {
                    continue;
                }

                visited.Add(position);

                var reachableNormally =
                    position.Around().Where(_ => maze.TryGetValue(_, out var t) && t.Type == TileType.Open);

                foreach (var p in reachableNormally)
                {
                    q.Enqueue((p, distance + 1));
                }

                // portals
                if (portals.TryGetValue(position, out var portalTarget))
                {
                    q.Enqueue((portalTarget, distance + 1));
                }
            }

            Assert.Equal(570, shortest);
        }

        [Fact]
        public void Test2()
        {
            var lines = File.ReadAllLines("input.txt");

            var maze = ParseMaze(lines);
            var (portals, start, end) = ConnectPortals(maze);

            var q = new Queue<(P, int, int)>();
            var visited = new HashSet<(P, int)>();

            q.Enqueue((start, 0, 0));
            var shortest = 0;

            const int minX = 2;
            const int minY = 2;
            var maxX = portals.Keys.Concat(portals.Values).Select(_ => _.X).Max();
            var maxY = portals.Keys.Concat(portals.Values).Select(_ => _.Y).Max();

            while (q.Any())
            {
                var (position, level, distance) = q.Dequeue();

                if (position.Equals(end) && level == 0)
                {
                    shortest = distance;
                    break;
                }

                if (visited.Contains((position, level)))
                {
                    continue;
                }
                else
                {
                    visited.Add((position, level));
                }

                var reachableNormally =
                    position.Around().Where(_ => maze.TryGetValue(_, out var t) && t.Type == TileType.Open);

                foreach (var p in reachableNormally)
                {
                    q.Enqueue((p, level, distance + 1));
                }

                // portals
                if (portals.TryGetValue(position, out var portalTarget))
                {
                    if (position.X == minX || position.X == maxX || position.Y == minY || position.Y == maxY)
                    {
                        // outer portal
                        if (level > 0)
                        {
                            q.Enqueue((portalTarget, level - 1, distance + 1));
                        }
                    }
                    else
                    {
                        // inner portal
                        q.Enqueue((portalTarget, level + 1, distance + 1));
                    }
                }
            }

            Assert.Equal(7056, shortest);
        }

        private (Dictionary<P, P> Portals, P Start, P End) ConnectPortals(Dictionary<P, Tile> maze)
        {
            var list = new Dictionary<string, HashSet<P>>();

            foreach (var (p, tile) in maze.Where(_ => _.Value.Type == TileType.Portal))
            {
                if (!p.Around().Any(_ => maze.TryGetValue(_, out var t) && t.Content == '.'))
                {
                    continue;
                };

                var otherTile = p.Around().Single(_ => maze.TryGetValue(_, out var t) && t.Type == TileType.Portal);

                var name = new string(new[] { tile.Content, maze[otherTile].Content }.OrderBy(_ => _).ToArray());

                if (list.TryGetValue(name, out var positions))
                {
                    positions.Add(p);
                }
                else
                {
                    list[name] = new HashSet<P> { p };
                }
            }

            var result = new Dictionary<P, P>();

            if (list.Any(_ => _.Value.Count > 2))
            {
                // I was lucky, there were no portal pairs called AB and BA, that would throw my portal connection off
                throw new Exception("Duplicate portal names detected");
            }

            foreach (var (_, value) in list.Where(_ => _.Value.Count == 2))
            {
                var openTile1 = value.First().Around()
                    .Single(_ => maze.TryGetValue(_, out var t) && t.Content == '.');
                var openTile2 = value.Last().Around()
                    .Single(_ => maze.TryGetValue(_, out var t) && t.Content == '.');

                result.Add(openTile1, openTile2);
                result.Add(openTile2, openTile1);
            }

            var startTile = list["AA"].Single().Around().Single(_ => maze.TryGetValue(_, out var t) && t.Content == '.');
            var endTile = list["ZZ"].Single().Around().Single(_ => maze.TryGetValue(_, out var t) && t.Content == '.');

            return (result, startTile, endTile);
        }

        private static Dictionary<P, Tile> ParseMaze(string[] lines)
        {
            var dictionary = new Dictionary<P, Tile>();

            for (var row = 0; row < lines.Length; row++)
            {
                for (var col = 0; col < lines[0].Length; col++)
                {
                    var p = new P { X = col, Y = row };

                    var tile = lines[row][col];

                    if (tile == ' ')
                    {
                        continue;
                    }

                    dictionary.Add(p, new Tile
                    {
                        Content = tile,
                        Type = tile switch
                        {
                            '#' => TileType.Wall,
                            '.' => TileType.Open,
                            char c when 'A' <= c && c <= 'Z' => TileType.Portal
                        }
                    });
                }
            }

            return dictionary;
        }

        private struct Tile
        {
            public char Content { get; set; }

            public TileType Type { get; set; }
        }

        [DebuggerDisplay("{X}, {Y}")]
        private struct P : IEquatable<P>
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
                return Equals((P)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }

            public IEnumerable<P> Around()
            {
                yield return new P { X = X, Y = Y - 1 };
                yield return new P { X = X - 1, Y = Y };
                yield return new P { X = X + 1, Y = Y };
                yield return new P { X = X, Y = Y + 1 };
            }
        }
    }

    internal enum TileType
    {
        Open,
        Wall,
        Portal
    }
}
