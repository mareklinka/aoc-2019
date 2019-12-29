using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace _13
{
    public class UnitTest1
    {
        [Fact]
        public void Part1()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, 10000);
            var finalScreen = RunPainting(program);

            Assert.Equal(193, finalScreen.Values.Count(_ => _ == TileType.Block));
        }

        [Fact]
        public void Part2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, 10000);

            var score = RunPainting2(program);

            Assert.Equal(10547, score);
        }

        private static int RunPainting2(long[] program)
        {
            P ballPos = null;
            P paddlePos = null;
            var score = 0;

            var outputBuffer = new List<long>();

            var io = new CustomIo(() => ballPos.X == paddlePos.X ? 0 : (ballPos.X < paddlePos.X ? -1 : 1), l =>
            {
                outputBuffer.Add(l);

                if (outputBuffer.Count == 3)
                {
                    var x = (int)outputBuffer[0];
                    var y = (int)outputBuffer[1];
                    var tile = (int)outputBuffer[2];

                    if (x == -1 && y == 0)
                    {
                        score = tile;
                    }
                    else
                    {
                        var p = new P { X = x, Y = y };

                        var tileType = (TileType)tile;
                        if (tileType == TileType.Ball)
                        {
                            ballPos = p;
                        }
                        else if (tileType == TileType.HorizontalPaddle)
                        {
                            paddlePos = p;
                        }
                    }

                    outputBuffer.Clear();
                }
            });

            var computer = new IntComputer(io);

            program[0] = 2;
            computer.ExecuteProgram(program);

            return score;
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

        private static Dictionary<P, TileType> RunPainting(long[] program)
        {
            Array.Resize(ref program, 10000);

            var input = new BlockingCollection<long>(new ConcurrentQueue<long>());
            var output = new BlockingCollection<long>(new ConcurrentQueue<long>());

            var computer = new IntComputer(input, output);

            var dictionary = new Dictionary<P, TileType>();

            computer.ExecuteProgram(program);

            while (output.Any())
            {
                var x = (int)output.Take();
                var y = (int)output.Take();
                var tile = output.Take();
                var p = new P { X = x, Y = y };

                if ((TileType)tile == TileType.Empty)
                {
                    if (dictionary.ContainsKey(p))
                    {
                        dictionary.Remove(p);
                    }
                }
                else
                {
                    dictionary[p] = (TileType)tile;
                }
            }
            
            return dictionary;
        }

        private enum TileType
        {
            Empty = 0,
            Wall = 1,
            Block = 2,
            HorizontalPaddle = 3,
            Ball = 4
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
