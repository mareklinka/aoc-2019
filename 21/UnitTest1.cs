using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace _21
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


            var input = new Queue<long>();
            var output = new List<long>();

            var computer = new IntComputer(new CustomIo(() => input.Dequeue(), l => output.Add(l)));

            var script = "NOT A T\nNOT B J\nOR T J\nNOT C T\nOR T J\nAND D J\nWALK\n";
            LoadScript(script, input);

            computer.ExecuteProgram(program, 0);

            if (output.Any(_ => _ > 128))
            {
                // success
                Assert.Equal(19354464, output.Max());
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var l in output)
                {
                    sb.Append((char) l);
                }

                _output.WriteLine(sb.ToString());
            }
        }

        [Fact]
        public void Part2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 3);

            var input = new Queue<long>();
            var output = new List<long>();

            var computer = new IntComputer(new CustomIo(() => input.Dequeue(), l => output.Add(l)));

            var script = "NOT A T\nNOT B J\nOR T J\nNOT C T\nOR T J\nNOT E T\nNOT T T\nAND I T\nOR H T\nAND T J\nNOT F T\nNOT T T\nAND E T\nOR T J\nAND D J\nRUN\n";
            LoadScript(script, input);

            computer.ExecuteProgram(program, 0);

            if (output.Any(_ => _ > 128))
            {
                // success
                Assert.Equal(19354464, output.Max());
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var l in output)
                {
                    sb.Append((char)l);
                }

                _output.WriteLine(sb.ToString());
            }
        }

        private void LoadScript(string script, Queue<long> target)
        {
            foreach (var l in script.Select(_ => (long)_))
            {
                target.Enqueue(l);
            }
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
