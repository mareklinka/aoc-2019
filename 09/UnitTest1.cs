using System;
using System.IO;
using System.Linq;
using Xunit;

namespace _09
{
    public class UnitTest1
    {
        [Fact]
        public void Part1()
        {
            var source = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            var program = source.Concat(new long[10000]).ToArray();
            var computer = new IntComputer();
            computer.IO.AddInput(1);
            computer.ExecuteProgram(program);

            Assert.Equal(3280416268, computer.IO.ReadOutput());
        }

        [Fact]
        public void Part2()
        {
            var source = File.ReadAllText("input.txt").Split(",").Select(long.Parse).ToArray();
            var program = source.Concat(new long[10000]).ToArray();
            var computer = new IntComputer();
            computer.IO.AddInput(2);
            computer.ExecuteProgram(program);

            Assert.Equal(80210, computer.IO.ReadOutput());
        }

        [Theory]
        [InlineData("109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99", "109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99")]
        [InlineData("1102,34915192,34915192,7,4,7,99,0", "1219070632396864")]
        [InlineData("104,1125899906842624,99", "1125899906842624")]
        public void TestLatestIntComputer(string program, string expected)
        {
            var computer = new IntComputer();
            var array = program.Split(",").Select(long.Parse).Concat(new long[1000]).ToArray();
            computer.ExecuteProgram(array);

            Assert.Equal(expected, string.Join(",", computer.IO.Output.ToArray()));
        }
    }
}
