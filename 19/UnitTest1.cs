using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace _19
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
            Array.Resize(ref program, program.Length * 3);

            var results = 0L;

            for (var row = 0; row < 50; ++row)
            {
                for (var col = 0; col < 50; ++col)
                {
                    var positions = new Queue<long>();

                    positions.Enqueue(col);
                    positions.Enqueue(row);

                    var io = new CustomIo(() => positions.Dequeue(), l => results += l);
                    var c = new IntComputer(io);

                    c.ExecuteProgram(program[..], 0);
                }
            }
        }

        [Theory]
        [InlineData("input.txt")]
        [InlineData("in.txt")]
        [InlineData("input.in")]
        [InlineData("p19.txt")]
        public void TestEquivalency(string file)
        {
            // I gathered some other inputs from reddit and used to validate optimizations
            // I compare the slow but deterministic solution to the binary search-based one
            // seems to be working correctly
            var program = File.ReadAllText(file).Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 3);

            var horizontalFit = FindHorizontalFit(200, 2000, 140, program);

            var (xr, yr) = FindSmallerRect(horizontalFit, horizontalFit + 1000, 0, program);
            (xr, yr) = FindVerticalFit(horizontalFit, program);

            var (x, y) = FindVerticalFit(horizontalFit, program);

            Assert.Equal(x, xr);
            Assert.Equal(y, yr);
        }

        [Fact]
        public void Part2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 3);

            // binary search for a line where a rectangle 99x100 (width x height) fits into the beam
            // given the nature of the beam, I'm exploiting the fact that the beam width on each row is non-decreasing (increases or stays same)
            // therefore if I find the line that fits 99x100, the line that fits 100x100 must be following closely
            // this does not work when directly searching for 100x100 because of the nature of binary search
            var (x, y) = FindSmallerRect(0, 3000, 0, program);

            // linear search starting at the previously found line to find the first one that fits 100x100
            (x, y) = FindVerticalFit(y, program);

            // visual sanity check
            //PrintResult(y, x, program);

            Assert.Equal(10671712, x * 10000 + y);
        }

        private void PrintResult(int y, int x, long[] program)
        {
            var sb = new StringBuilder();

            for (var row = y - 5; row < y + 105; ++row)
            {
                for (var col = x - 5; col < x + 105; ++col)
                {
                    var result = CalculatePoint(col, row, program);

                    if (row >= y && row < y + 100 && col >= x && col < x + 100)
                    {
                        if (result == 1)
                        {
                            sb.Append('X');
                        }
                        else
                        {
                            sb.Append('0');
                        }
                    }
                    else
                    {
                        sb.Append(result == 1 ? '#' : '.');
                    }
                }

                sb.AppendLine();
            }

            _output.WriteLine(sb.ToString());
        }

        private static long CalculatePoint(int col, int row, long[] program)
        {
            var result = 0L;
            var positions = new Queue<long>();

            positions.Enqueue(col);
            positions.Enqueue(row);

            var io = new CustomIo(() => positions.Dequeue(), l => result = l);
            var c = new IntComputer(io);

            c.ExecuteProgram(program[..], 0);
            return result;
        }

        private static (int, int) FindVerticalFit(int startRow, long[] program)
        {
            // this is a linear search that starts at the given row and looks for the first row that can fit a 100x100 object
            // this is my original solution to part 2 - not too smart but runs in about 20s
            var positions = new Queue<long>();
            for (var row = startRow; row < startRow + 1000; ++row)
            {
                var result = 0L;
                var beam = CalculateBeam(row, 0, program);
                var col = beam.FirstColumn + beam.Width - 100;
                positions.Enqueue(col);
                positions.Enqueue(row + 100 - 1); // this subtle -1 is key - we want height of 100, that means the last row of the rectangle is row + 99

                var io = new CustomIo(() => positions.Dequeue(), l => { result = l; });
                var c = new IntComputer(io);

                c.ExecuteProgram(program[..], 0);

                if (result == 1)
                {
                    return (col, row);
                }
            }

            throw new Exception();
        }

        private static (int, int) FindSmallerRect(int start, int end, int offset, long[] program)
        {
            // this is part of the binary search solution - searches for a line that can fit a 99x100 object
            // works by calculating beam widths from line L and L + 99
            // then it calculates the maximum width of a rectangle that would fit between those lines and recurses
            if (start == end || start == end - 1)
            {
                var b = CalculateBeam(start, offset, program);
                return (b.FirstColumn + b.Width - 100, start);
            }

            var mid = (end + start) / 2;

            var beam1 = CalculateBeam(mid, offset, program);
            var beam2 = CalculateBeam(mid + 100 - 1, offset, program);

            var maxX = beam1.FirstColumn + beam1.Width;
            var minX = beam2.FirstColumn;

            if (maxX - minX >= 99)
            {
                return FindSmallerRect(start, mid, beam1.FirstColumn / 2, program);
            }
            else
            {
                return FindSmallerRect(mid, end, beam1.FirstColumn - 1, program);
            }
        }

        private static int FindHorizontalFit(int start, int end, int threshold, long[] program)
        {
            // binary search for a line that has a beam width of threshold
            if (start == end || start == end - 1)
            {
                return start;
            }

            var mid = (end + start) / 2;

            var (width, _) = CalculateBeam(mid, 0, program);

            if (width >= threshold)
            {
                return FindHorizontalFit(start, mid, threshold, program);
            }
            else
            {
                return FindHorizontalFit(mid, end, threshold, program);
            }
        }

        private static (int Width, int FirstColumn) CalculateBeam(int row, int offset, long[] program)
        {
            // calculates the beam start and width at a given row
            // starts computing at offset to exploit the fact that that beam's starting position is moving from left to right
            var col = 0;
            var inBeam = false;
            var beamWidth = 0;
            var beamStart = -1;

            while (true)
            {
                var result = CalculatePoint(col++ + offset, row, program);

                if (result == 1)
                {
                    beamWidth++;
                }

                if (result == 1 && !inBeam)
                {
                    beamStart = col - 1;
                    inBeam = true;
                }

                if (result == 0 && inBeam)
                {
                    break;
                }
            }

            return (beamWidth, beamStart);
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

            public long[] ReadAllOutput()
            {
                throw new NotImplementedException();
            }

            public void Write(long value)
            {
                _writeOp(value);
            }
        }

    }
}