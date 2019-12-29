using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace _25
{
    class Program
    {
        static void Main(string[] args)
        {
            // items required to pass the checkpoint:
            // brochure, heater, hologram, cat 6
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 3);

            var input = new BlockingCollection<long>(new ConcurrentQueue<long>());
            var io = new ConsoleIo(input, GameOutput);
            var computer = new IntComputer(io);

            Task.Run(() => computer.ExecuteProgram(program, 0));

            while (true)
            {
                Console.WriteLine();
                var command = Console.ReadLine();

                foreach (var l in command.Select(_ => (long)_))
                {
                    input.Add(l);
                }

                input.Add('\n');
            }
        }

        private static void GameOutput(string s)
        {
            Console.Write(s);
        }

        private class ConsoleIo : IntComputer.IInputOutput
        {
            private readonly BlockingCollection<long> _input;
            private readonly Action<string> _output;

            public ConsoleIo(BlockingCollection<long> input, Action<string> output)
            {
                _input = input;
                _output = output;
            }

            public long Read()
            {
                return _input.Take();
            }

            public long[] ReadAllOutput()
            {
                throw new NotImplementedException();
            }

            private readonly List<long> _outputBuffer = new List<long>();

            public void Write(long value)
            {
                _outputBuffer.Add(value);

                if (value == '\n')
                {
                    _output(new string(_outputBuffer.Select(_ => (char) _).ToArray()));
                    _outputBuffer.Clear();
                }
            }
        }
    }
}
