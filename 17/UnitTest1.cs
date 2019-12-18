using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace _17
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
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 5);

            var computer = new IntComputer();

            computer.ExecuteProgram(program, 0);

            var sb = new StringBuilder();

            foreach (char c in computer.IO.ReadAllOutput())
            {
                sb.Append(c);
            }

            var map = sb.ToString();
            _output.WriteLine(map);

            var lines = map.Trim().Split('\n');

            var sum = 0;

            for (var row = 1; row < lines.Length - 1; ++row)
            {
                for (var col = 1; col < lines[0].Length - 1; col++)
                {
                    sum += GetAlignmentParameter(lines, row, col);
                }
            }

            Assert.Equal(6000, sum);
        }

        [Fact]
        public void Part2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 5);

            var computer = new IntComputer();

            computer.ExecuteProgram(program[..], 0);

            var sb = new StringBuilder();

            foreach (char c in computer.IO.ReadAllOutput())
            {
                sb.Append(c);
            }

            var map = sb.ToString();
            var lines = map.Trim().Split('\n');
            var startLine = lines.Single(_ => _.Contains('^'));

            // I did this by hand, lol :D
            var sequence = "A,B,A,B,A,C,B,C,A,C";
            var commands = new Dictionary<string, string>
            {
                { "A", "R,4,L,10,L,10" },
                { "B", "L,8,R,12,R,10,R,4" },
                { "C", "L,8,L,8,R,10,R,4" }
            }; ;

            // MoveAndDraw(lines, startLine, sequence, commands, map); // for evaluation only

            var intSequence = sequence.ToCharArray().Select(_ => (long) _).ToArray();
            var commandA = commands["A"].ToCharArray().Select(_ => (long) _).ToArray();
            var commandB = commands["B"].ToCharArray().Select(_ => (long) _).ToArray();
            var commandC = commands["C"].ToCharArray().Select(_ => (long) _).ToArray();
            var nl = new long[] { 10 };
            var videoFeed = new long[] { 'n' };

            var seq = intSequence.Concat(nl).Concat(commandA)
                .Concat(nl).Concat(commandB).Concat(nl).Concat(commandC).Concat(nl).Concat(videoFeed).Concat(nl).ToArray();
            var input = new BlockingCollection<long>(new ConcurrentQueue<long>(seq));
            computer = new IntComputer(input, new BlockingCollection<long>(new ConcurrentQueue<long>()));
            program[0] = 2;
            computer.ExecuteProgram(program, 0);
            var result = computer.IO.ReadAllOutput().Last();

            Assert.Equal(807320, result);
        }

        private void MoveAndDraw(string[] lines, string startLine, string sequence, Dictionary<string, string> commands, string map)
        {
            var direction = Direction.North;
            var position = new P { Y = Array.IndexOf(lines, startLine), X = startLine.IndexOf("^") };

            foreach (var c in sequence.Split(","))
            {
                var command = commands[c];

                foreach (var s in command.Split(","))
                {
                    switch (s)
                    {
                        case "R":
                            direction = direction switch
                            {
                                Direction.North => Direction.East,
                                Direction.East => Direction.South,
                                Direction.South => Direction.West,
                                Direction.West => Direction.North
                            };
                            break;
                        case "L":
                            direction = direction switch
                            {
                                Direction.North => Direction.West,
                                Direction.West => Direction.South,
                                Direction.South => Direction.East,
                                Direction.East => Direction.North
                            };
                            break;
                        default:
                            var step = int.Parse(s);
                            for (var i = 0; i < step; ++i)
                            {
                                position = position.Move(direction);
                                var charArray = lines[position.Y].ToCharArray();
                                charArray[position.X] = 'X';
                                lines[position.Y] = new string(charArray);
                            }

                            map = string.Join("\n", lines);

                            break;
                    }
                }
            }

            _output.WriteLine(map);
        }

        private static int GetAlignmentParameter(string[] lines, in int row, in int col)
        {
            var isIntersection = lines[row][col] == '#' &&
                                 lines[row - 1][col] == '#' &&
                                 lines[row + 1][col] == '#' &&
                                 lines[row][col - 1] == '#' &&
                                 lines[row][col + 1] == '#';

            if (!isIntersection)
            {
                return 0;
            }

            return row * col;
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
                return Equals((P)obj);
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
