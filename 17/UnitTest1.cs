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
            
            var instructions = GetMovementInstructions(map);

            for (var length1 = 1; length1 < instructions.Count - 2; ++length1)
            {
                var part1 = instructions.Take(length1).ToList();
                var remainder1 = Remove(instructions, part1);

                for (var length2 = 1; length2 < remainder1.Count - 1; ++length2)
                {
                    var part2 = remainder1.Take(length2).ToList();
                    var remainder2 = Remove(remainder1, part2);

                    for (var length3 = 1; length3 < remainder2.Count; ++length3)
                    {
                        var part3 = remainder2.Take(length3).ToList();

                        var remainder = Remove(remainder2, part3);

                        if (remainder.Count == 0)
                        {
                            var list = new List<char>();
                            for (var i = 0; i < instructions.Count; ++i)
                            {
                                if (instructions.Skip(i).Take(part1.Count).SequenceEqual(part1))
                                {
                                    list.Add('A');
                                    i = i + part1.Count - 1;
                                    continue;
                                }

                                if (instructions.Skip(i).Take(part2.Count).SequenceEqual(part2))
                                {
                                    list.Add('B');
                                    i = i + part2.Count - 1;
                                    continue;
                                }

                                if (instructions.Skip(i).Take(part3.Count).SequenceEqual(part3))
                                {
                                    list.Add('C');
                                    i = i + part2.Count - 1;
                                }
                            }

                            var sequence = string.Join(",", list);
                            var commands = new Dictionary<string, string>
                            {
                                { "A", Stringify(part1) },
                                { "B", Stringify(part2) },
                                { "C", Stringify(part3) }
                            };

                            // MoveAndDraw(lines, startLine, sequence, commands, map); // for evaluation only

                            var intSequence = sequence.ToCharArray().Select(_ => (long)_).ToArray();
                            var commandA = commands["A"].ToCharArray().Select(_ => (long)_).ToArray();
                            var commandB = commands["B"].ToCharArray().Select(_ => (long)_).ToArray();
                            var commandC = commands["C"].ToCharArray().Select(_ => (long)_).ToArray();
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

                            return;
                        }
                    }
                }
            }
        }

        private static List<Instruction> GetMovementInstructions(string map)
        {
            var distance = 0;
            char? turn = null;

            var result = new List<Instruction>();

            var lines = map.Trim().Split('\n');
            var startY = Array.IndexOf(lines, lines.Single(_ => _.Contains('^')));
            var startX = lines[startY].IndexOf('^');

            var dir = Direction.North;

            var pos = new P { X = startX, Y = startY };

            while (pos != null)
            {
                var prevPos = pos;
                var inDirection = pos.Move(dir);

                if (inDirection.X < 0 || inDirection.X >= lines[0].Length || inDirection.Y < 0 || inDirection.Y >= lines.Length)
                { 
                }
                else if(lines[inDirection.Y][inDirection.X] == '#')
                {
                    distance++;
                    pos = inDirection;
                    continue;
                }

                var otherDirections = dir switch
                {
                    Direction.North => new[] { Direction.West, Direction.East },
                    Direction.West => new[] { Direction.South, Direction.North },
                    Direction.South => new[] { Direction.East, Direction.West },
                    Direction.East => new[] { Direction.North, Direction.South },
                };

                pos = null;

                foreach (var od in otherDirections)
                {
                    inDirection = prevPos.Move(od);

                    if (inDirection.X < 0 || inDirection.X >= lines[0].Length || inDirection.Y < 0 || inDirection.Y >= lines.Length)
                    {
                        continue;
                    }

                    if (lines[inDirection.Y][inDirection.X] == '#')
                    {
                        var newTurn = (dir, od) switch
                        {
                            (Direction.North, Direction.East) => 'R',
                            (Direction.North, Direction.West) => 'L',

                            (Direction.West, Direction.North) => 'R',
                            (Direction.West, Direction.South) => 'L',

                            (Direction.South, Direction.West) => 'R',
                            (Direction.South, Direction.East) => 'L',

                            (Direction.East, Direction.South) => 'R',
                            (Direction.East, Direction.North) => 'L',
                        };

                        dir = od;

                        if (turn != null)
                        {
                            result.Add(new Instruction { Distance = distance, Turn = turn.Value });
                        }

                        turn = newTurn;

                        distance = 1; // after turn we also moved one in that direction
                        pos = inDirection;
                    }
                }
            }

            result.Add(new Instruction { Distance = distance, Turn = turn.Value });

            return result;
        }

        private string Stringify(IEnumerable<Instruction> instructions)
        {
            return string.Join(",", instructions.Select(_ => $"{_.Turn},{_.Distance}"));
        }

        private List<Instruction> Remove(List<Instruction> haystack, List<Instruction> needle)
        {
            if (haystack.Count < needle.Count)
            {
                return haystack.ToList();
            }

            var result = new List<Instruction>();

            for (var haystackPointer = 0; haystackPointer < haystack.Count; ++haystackPointer)
            {
                if (haystack.Skip(haystackPointer).Take(needle.Count).SequenceEqual(needle))
                {
                    haystackPointer = haystackPointer + needle.Count - 1;
                    continue;
                }

                result.Add(haystack[haystackPointer]);
            }

            return result;
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

        [DebuggerDisplay("{Turn}, {Distance}")]
        private class Instruction : IEquatable<Instruction>
        {
            public char Turn { get; set; }

            public int Distance { get; set; }

            public bool Equals(Instruction other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Turn == other.Turn && Distance == other.Distance;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Instruction) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Turn, Distance);
            }
        }
    }
}
