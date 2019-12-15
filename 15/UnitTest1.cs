using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace _15
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();

            var result = FindTank(program);

            Assert.Equal(212, result.Steps);
        }

        [Fact]
        public void Part2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();

            var maxDistance = FindLongestShortestDistance(program);

            Assert.Equal(358, maxDistance);
        }

        private static int FindLongestShortestDistance(long[] program)
        {
            // the number of minutes it takes to fill the area with oxygen is the shortest distance between the tank and the most distant tile
            // therefore we are looking for the longest shortest route on the map
            var (_, map) = FindTank(program);

            // starting point
            var tank = map.Single(_ => _.Value == ReturnCode.Tank);

            var q = new Queue<(P, int)>();
            q.Enqueue((tank.Key, 0));

            var maxDistance = 0;
            var visited = new HashSet<P>();

            while (q.Any())
            {
                var (p, d) = q.Dequeue();

                if (visited.Contains(p))
                {
                    continue;
                }
                else
                {
                    visited.Add(p);
                }

                maxDistance = Math.Max(d, maxDistance); // looking for a max

                foreach (Direction direction in new[] {1, 2, 3, 4})
                {
                    // look around the current point for non-wall tiles for further exploration
                    var moved = p.Move(direction);
                    if (map.TryGetValue(moved, out var tile) && tile != 0)
                    {
                        q.Enqueue((moved, d + 1));
                    }
                }
            }

            return maxDistance;
        }

        private static (long Steps, Dictionary<P, ReturnCode> Map) FindTank(long[] program)
        {
            // to find the tank, we need to do a breadth-first search -
            // we are looking for a shortest distance between the start and the oxygen tank
            // for Part 2, we don't stop when we find the tank but continue exploring the map until we visited every reachable tile
            long? numberOfSteps = null;

            var visited = new Dictionary<P, ReturnCode> {{new P(), ReturnCode.Moved}};
            var q = new Queue<(long[] Program, long Pointer, P Position, long Distance)>();
            
            q.Enqueue((program, 0, new P(), 0));

            while (q.Any())
            {
                var (prg, pointer, pos, d) = q.Dequeue();

                var result = 0L;

                foreach (Direction direction in new[] {1,2,3,4})
                {
                    // to do a breadth-first search with an int computer, we set the computer to terminate 
                    // after each write instruction - that will ensure we moved by one position
                    // we also need to be able to store and restore the current computer state (the memory state and instruction pointer)

                    var io = new CustomIo(() => (long)direction, l => result = l);
                    var c = new IntComputer(io) { TerminateAfterOutput = true };
                    c.ExecuteProgram(prg[..], pointer); // this moves us by one tile only

                    var moved = pos.Move(direction);

                    if (result != 0)
                    {
                        if (result == 2 && numberOfSteps is null)
                        {
                            // this is the shortest distance to the tank
                            numberOfSteps = d + 1;
                        }

                        if (!visited.ContainsKey(moved))
                        {
                            // we moved into an empty tile we haven't seen before - we enqueue it for further searching
                            visited.Add(moved, (ReturnCode)result);
                            q.Enqueue((c.Memory, c.InstructionPointer, moved, d + 1));
                        }
                    }
                    else
                    {
                        if (!visited.ContainsKey(moved))
                        {
                            // we moved into a wall - no need to process further but we add it to the map of the area
                            visited.Add(moved, (ReturnCode)result);
                        }
                    }
                }
            }

            return (numberOfSteps.Value, visited);
        }

        public class CustomIo : IntComputer.IInputOutput
        {
            private readonly Func<long> _readOp;
            private readonly Action<long> _writeOp;

            public CustomIo(Func<long> readOp, Action<long> writeOp)
            {
                _readOp = readOp;
                _writeOp = writeOp;
            }

            public long Read()
            {
                return _readOp();
            }

            public void Write(long value)
            {
                _writeOp(value);
            }
        }

        private enum ReturnCode
        {
            Wall = 0,
            Moved = 1,
            Tank = 2
        }

        private enum Direction
        {
            North = 1,
            South = 2,
            West = 3,
            East = 4
        }

        [DebuggerDisplay("{X}, {Y}")]
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
                return HashCode.Combine(X, Y);
            }

            public P Move(Direction direction)
            {
                return direction switch
                {
                    Direction.North => new P { X = X, Y = Y - 1 },
                    Direction.East => new P { X = X + 1, Y = Y },
                    Direction.South => new P { X = X, Y = Y + 1 },
                    Direction.West => new P { X = X - 1, Y = Y }
                };
            }
        }
    }
}
