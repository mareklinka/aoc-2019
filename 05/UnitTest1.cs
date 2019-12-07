using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace _05
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
            var program = File.ReadAllText("input.txt").Split(",").Select(int.Parse).ToArray();

            var io = new IntComputer.InputOutput(Enumerable.Range(1, 1), _output);
            var computer = new IntComputer(io);
            computer.ExecuteProgram(program);

            io.Dump();
        }

        [Fact]
        public void Part2()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(int.Parse).ToArray();

            var io = new IntComputer.InputOutput(Enumerable.Range(5, 1), _output);
            var computer = new IntComputer(io);
            computer.ExecuteProgram(program);

            io.Dump();
        }

        [Theory]
        [InlineData("3,9,8,9,10,9,4,9,99,-1,8", 1, 0)]
        [InlineData("3,9,8,9,10,9,4,9,99,-1,8", 8, 1)]
        [InlineData("3,9,8,9,10,9,4,9,99,-1,8", 9, 0)]
        [InlineData("3,9,7,9,10,9,4,9,99,-1,8", 1, 1)]
        [InlineData("3,9,7,9,10,9,4,9,99,-1,8", 8, 0)]
        [InlineData("3,9,7,9,10,9,4,9,99,-1,8", 9, 0)]
        [InlineData("3,3,1108,-1,8,3,4,3,99", 1, 0)]
        [InlineData("3,3,1108,-1,8,3,4,3,99", 8, 1)]
        [InlineData("3,3,1108,-1,8,3,4,3,99", 9, 0)]
        [InlineData("3,3,1107,-1,8,3,4,3,99", 1, 1)]
        [InlineData("3,3,1107,-1,8,3,4,3,99", 8, 0)]
        [InlineData("3,3,1107,-1,8,3,4,3,99", 9, 0)]
        [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", 0, 0)]
        [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", 1, 1)]
        [InlineData("3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9", -6, 1)]
        [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", 0, 0)]
        [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", 1, 1)]
        [InlineData("3,3,1105,-1,9,1101,0,0,12,4,12,99,1", -6, 1)]
        [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", -6, 999)]
        [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 8, 1000)]
        [InlineData("3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99", 10, 1001)]
        public void TestProgramExecution(string programString, int input, int expected)
        {
            var program = programString.Split(",").Select(int.Parse).ToArray();

            var io = new IntComputer.InputOutput(Enumerable.Range(input, 1), _output);
            var computer = new IntComputer(io);
            computer.ExecuteProgram(program);

            Assert.Equal(expected, io.LastOutput);
        }
    }
    
}
