using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace _11
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
            var visited = RunPainting(0);

            Assert.Equal(1964, visited.Count);
        }

        [Fact]
        public void Part2()
        {
            var visited = RunPainting(1);

            var minW = visited.Min(_ => _.Key.X);
            var minH = visited.Min(_ => _.Key.Y);
            var maxW = visited.Max(_ => _.Key.X);
            var maxH = visited.Max(_ => _.Key.Y);

            var sb = new StringBuilder();

            for (var row = minH; row <= maxH; row++)
            {
                for (var col = minW; col <= maxW; col = ++col)
                {
                    if (!visited.TryGetValue(new P { X = col, Y = row }, out var color) || !color)
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append('█');
                    }
                }

                sb.Append('\n');
            }
            
            _output.WriteLine(sb.ToString());
        }

        private static Dictionary<P, bool> RunPainting(int initialColor)
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, 10000);

            var input = new BlockingCollection<long>(new ConcurrentQueue<long>());
            var output = new BlockingCollection<long>(new ConcurrentQueue<long>());
            input.Add(initialColor);

            var computer = new IntComputer(input, output);

            var direction = Direction.Up;
            var position = new P {X = 0, Y = 0};
            var visited = new Dictionary<P, bool>();

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var computerTask = Task.Run(() => computer.ExecuteProgram(program));

            var movementTask = Task.Run(() =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var color = output.Take(token);
                        var rotation = output.Take(token);

                        visited[position] = color == 1;

                        direction = (direction, rotation) switch
                        {
                            (Direction.Up, 0) => Direction.Left,
                            (Direction.Up, 1) => Direction.Right,

                            (Direction.Left, 0) => Direction.Down,
                            (Direction.Left, 1) => Direction.Up,

                            (Direction.Down, 0) => Direction.Right,
                            (Direction.Down, 1) => Direction.Left,

                            (Direction.Right, 0) => Direction.Up,
                            (Direction.Right, 1) => Direction.Down,
                            _ => throw new Exception()
                        };

                        position = direction switch
                        {
                            Direction.Up => new P {X = position.X, Y = position.Y - 1},
                            Direction.Down => new P {X = position.X, Y = position.Y + 1},
                            Direction.Left => new P {X = position.X - 1, Y = position.Y},
                            Direction.Right => new P {X = position.X + 1, Y = position.Y},
                            _ => throw new Exception()
                        };

                        if (visited.TryGetValue(position, out var c))
                        {
                            input.Add(c ? 1 : 0);
                        }
                        else
                        {
                            input.Add(0);
                        }
                    }
                }
                catch (OperationCanceledException e)
                {
                }
            });

            Task.WaitAll(computerTask);

            cts.Cancel();

            Task.WaitAll(movementTask);
            return visited;
        }

        private enum Direction
        {
            Up,
            Right,
            Down,
            Left
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
                return Equals((P)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ Y;
                }
            }
        }
    }
}
