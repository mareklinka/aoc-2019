using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace _23
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // since I decided to exploit multithreading here, all computers run in parallel
            // this requires some throttling so that I don't starve the CPU
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 3);

            var list = new Dictionary<int, IntComputer>();

            var are = new AutoResetEvent(false);
            var finalY = 0L;

            for (var i = 0; i < 50; ++i)
            {
                list.Add(i, new IntComputer(new NetworkIO(i, (address, x, y) =>
                {
                    if (address == 255)
                    {
                        finalY = y;
                        are.Set();
                        return;
                    }

                    var io = ((NetworkIO) list[(int)address].IO);
                    io.QueueInput(x, y);
                })));
            }

            foreach (var c in list.Values)
            {
                Task.Run(() => c.ExecuteProgram(program[..], 0));
            }

            are.WaitOne();

            Assert.Equal(21089, finalY);
        }

        [Fact]
        public void Test2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            Array.Resize(ref program, program.Length * 3);

            var list = new Dictionary<int, IntComputer>();

            var are = new AutoResetEvent(false);
            var natX = 0L;
            var natY = 0L;

            var previousFromNat = -1L;
            var firstDuplicate = 0l;

            for (var i = 0; i < 50; ++i)
            {
                list.Add(i, new IntComputer(new NetworkIO(i, (address, x, y) =>
                {
                    if (address == 255)
                    {
                        natX = x;
                        natY = y;

                        return;
                    }

                    var io = ((NetworkIO)list[(int)address].IO);
                    io.QueueInput(x, y);
                })));
            }

            foreach (var c in list.Values)
            {
                Task.Run(() => c.ExecuteProgram(program[..], 0));
            }

            Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(100).GetAwaiter().GetResult();

                    var idle1 = list.All(_ => ((NetworkIO) _.Value.IO).IsIdle());
                    Task.Delay(400).GetAwaiter().GetResult();
                    var idle2 = list.All(_ => ((NetworkIO)_.Value.IO).IsIdle());

                    if (idle1 && idle2)
                    {
                        if (natY == previousFromNat)
                        {
                            firstDuplicate = natY;
                            are.Set();
                            return;
                        }

                        previousFromNat = natY;
                        ((NetworkIO)list[0].IO).QueueInput(natX, natY);
                    }
                }
            });

            are.WaitOne();

            Assert.Equal(16658, firstDuplicate);
        }

        private class NetworkIO : IntComputer.IInputOutput
        {
            private readonly long _address;
            private readonly Action<long, long, long> _writer;
            private readonly Queue<(long, long)> _packets = new Queue<(long, long)>();
            private readonly Queue<long> _inputs = new Queue<long>();
            private readonly Queue<long> _outputs = new Queue<long>();
            private bool _isFirstRead = true;

            public NetworkIO(long address, Action<long, long, long> writer)
            {
                _address = address;
                _writer = writer;
            }

            public bool IsIdle()
            {
                return _packets.Count == 0 && _outputs.Count == 0;
            }

            public void QueueInput(long x, long y)
            {
                _packets.Enqueue((x,y));
            }

            private bool _missedRead;

            public long Read()
            {
                if (_isFirstRead)
                {
                    _isFirstRead = false;
                    return _address;
                }

                if (_inputs.TryDequeue(out var i))
                {
                    _missedRead = false;
                    return i;
                }

                if (_packets.TryDequeue(out var p))
                {
                    _inputs.Enqueue(p.Item1);
                    _inputs.Enqueue(p.Item2);
                    return Read();
                }

                if (_missedRead)
                {
                    Task.Delay(30).GetAwaiter().GetResult();
                }

                _missedRead = true;

                return -1;
            }

            public long[] ReadAllOutput()
            {
                throw new NotImplementedException();
            }

            public void Write(long value)
            {
                if (_outputs.Count == 2)
                {
                    _writer(_outputs.Dequeue(), _outputs.Dequeue(), value);
                }
                else
                {
                    _outputs.Enqueue(value);
                }
            }
        }
    }
}
