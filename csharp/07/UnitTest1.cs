using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace _07
{
    public class UnitTest1
    {
        [Fact]
        public void Part1()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(int.Parse).ToArray();

            var max = CalculateMaxThrusterOutput(program);

            Assert.Equal(17440, max);
        }

        [Fact]
        public void Part2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(int.Parse).ToArray();

            var max = CalculateMaxThrustWithFeedback(program);

            Assert.Equal(27561242, max);
        }

        [Theory]
        [InlineData("3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0", 43210)]
        [InlineData("3,23,3,24,1002,24,10,24,1002,23,-1,23,101,5,23,23,1,24,23,23,4,23,99,0,0", 54321)]
        [InlineData("3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0", 65210)]
        public void TestThrusters(string programString, int expected)
        {
            var program = programString.Split(",").Select(int.Parse).ToArray();

            var max = CalculateMaxThrusterOutput(program);

            Assert.Equal(expected, max);
        }

        private static int CalculateMaxThrusterOutput(int[] program)
        {
            var maxThrust = int.MinValue;

            foreach (var phaseSetting in GetPhaseSettings(Enumerable.Range(0, 5), 5))
            {
                var thrust = EvaluatePhaseVariant(program, phaseSetting.ToArray());
                if (thrust > maxThrust)
                {
                    maxThrust = thrust;
                }
            }

            return maxThrust;
        }

        private static int EvaluatePhaseVariant(int[] program, int[] phases)
        {
            var amp1 = new IntComputer();
            amp1.IO.AddInput(phases[0]);
            amp1.IO.AddInput(0);
            amp1.ExecuteProgram(program.ToArray());

            var amp2 = new IntComputer();
            amp2.IO.AddInput(phases[1]);
            amp2.IO.AddInput(amp1.IO.ReadOutput());
            amp2.ExecuteProgram(program.ToArray());

            var amp3 = new IntComputer();
            amp3.IO.AddInput(phases[2]);
            amp3.IO.AddInput(amp2.IO.ReadOutput());
            amp3.ExecuteProgram(program.ToArray());

            var amp4 = new IntComputer();
            amp4.IO.AddInput(phases[3]);
            amp4.IO.AddInput(amp3.IO.ReadOutput());
            amp4.ExecuteProgram(program.ToArray());

            var amp5 = new IntComputer();
            amp5.IO.AddInput(phases[4]);
            amp5.IO.AddInput(amp4.IO.ReadOutput());
            amp5.ExecuteProgram(program.ToArray());

            var finalValue = amp5.IO.ReadOutput();
            return finalValue;
        }

        [Theory]
        [InlineData("3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5", 139629729)]
        [InlineData("3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54,-5,54,1105,1,12,1,53,54,53,1008,54,0,55,1001,55,1,55,2,53,55,53,4,53,1001,56,-1,56,1005,56,6,99,0,0,0,0,10", 18216)]
        public void TestThrustersWithFeedback(string programString, int expected)
        {
            var program = programString.Split(",").Select(int.Parse).ToArray();

            var max = CalculateMaxThrustWithFeedback(program);

            Assert.Equal(expected, max);
        }

        [Theory]
        [InlineData("3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5", "9,8,7,6,5", 139629729)]
        [InlineData("3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54,-5,54,1105,1,12,1,53,54,53,1008,54,0,55,1001,55,1,55,2,53,55,53,4,53,1001,56,-1,56,1005,56,6,99,0,0,0,0,10", "9,7,8,5,6", 18216)]
        public void TestThrustersWithFeedbackPhase(string programString, string phase, int expected)
        {
            var program = programString.Split(",").Select(int.Parse).ToArray();

            var value = EvaluatePhaseWithFeedback(program, phase.Split(",").Select(int.Parse).ToArray());

            Assert.Equal(expected, value);
        }

        private static int CalculateMaxThrustWithFeedback(int[] program)
        {
            var maxThrust = int.MinValue;

            foreach (var p in GetPhaseSettings(Enumerable.Range(5, 5), 5))
            {
                var thrust = EvaluatePhaseWithFeedback(program, p.ToArray());
                if (thrust > maxThrust)
                {
                    maxThrust = thrust;
                }
            }

            return maxThrust;
        }

        private static int EvaluatePhaseWithFeedback(int[] program, int[] phases)
        {
            var input1 = new BlockingCollection<int>(new ConcurrentQueue<int>(new[] {phases[0], 0})); // also output 5
            var input2 = new BlockingCollection<int>(new ConcurrentQueue<int>(new[] {phases[1]})); // also output 1
            var input3 = new BlockingCollection<int>(new ConcurrentQueue<int>(new[] {phases[2]})); // also output 2
            var input4 = new BlockingCollection<int>(new ConcurrentQueue<int>(new[] {phases[3]})); // also output 3
            var input5 = new BlockingCollection<int>(new ConcurrentQueue<int>(new[] {phases[4]})); // also output 4

            var amp1 = new IntComputer(input1, input2);
            var amp2 = new IntComputer(input2, input3);
            var amp3 = new IntComputer(input3, input4);
            var amp4 = new IntComputer(input4, input5);
            var amp5 = new IntComputer(input5, input1);

            var t1 = Task.Run(() => amp1.ExecuteProgram(program.ToArray()));
            var t2 = Task.Run(() => amp2.ExecuteProgram(program.ToArray()));
            var t3 = Task.Run(() => amp3.ExecuteProgram(program.ToArray()));
            var t4 = Task.Run(() => amp4.ExecuteProgram(program.ToArray()));
            var t5 = Task.Run(() => amp5.ExecuteProgram(program.ToArray()));

            Task.WaitAll(t1, t2, t3, t4, t5);

            var finalValue = amp5.IO.ReadOutput();
            return finalValue;
        }

        private static IEnumerable<IEnumerable<int>> GetPhaseSettings(IEnumerable<int> list, int length)
        {
            if (length == 1)
            {
                return list.Select(t => new[] { t });
            }

            return GetPhaseSettings(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new[] {t2}));
        }
    }
}
