using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace _02
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Part1()
        {
            var program = File.ReadAllText("input.txt").Split(",").Select(int.Parse).ToArray();

            program[1] = 12;
            program[2] = 2;

            var result = InterpretProgram(program);

            output.WriteLine(result.ToString());
        }

        [Fact]
        public void Part2()
        {
            var originalProgram = File.ReadAllText("input.txt").Split(",").Select(int.Parse).ToArray();

            foreach (var noun in Enumerable.Range(0, 100))
            {
                foreach (var verb in Enumerable.Range(0, 100))
                {
                    var program = originalProgram[..];

                    program[1] = noun;
                    program[2] = verb;

                    var result = InterpretProgram(program);

                    if (result == 19690720)
                    {
                        output.WriteLine($"{noun}");
                        output.WriteLine($"{verb}");
                        output.WriteLine($"{100 * noun + verb}");
                        return;
                    }
                }
            }
        }

        [Theory]
        [InlineData("1,0,0,0,99", "2,0,0,0,99")]
        [InlineData("2,3,0,3,99", "2,3,0,6,99")]
        [InlineData("2,4,4,5,99,0", "2,4,4,5,99,9801")]
        [InlineData("1,1,1,4,99,5,6,0,99", "30,1,1,4,2,5,6,0,99")]
        public void TestProgram(string programString, string expected)
        {
            var program = programString.Split(",").Select(int.Parse).ToArray();

            InterpretProgram(program);

            Assert.Equal(expected, string.Join(",", program));
        }


        private int InterpretProgram(int[] program)
        {
            var pointer = 0;

            while (InterpretInstruction(program, pointer))
            {
                pointer += 4;
            }

            return program[0];
        }

        private bool InterpretInstruction(int[] program, int pointer)
        {
            var instruction = program[pointer];

            if (instruction == 99)
            {
                return false;
            }

            var indexA = program[pointer + 1];
            var indexB = program[pointer + 2];
            var indexResult = program[pointer + 3];

            switch (instruction)
            {
                case 1:
                    program[indexResult] = program[indexA] + program[indexB];
                    return true;
                case 2:
                    program[indexResult] = program[indexA] * program[indexB];
                    return true;
                default:
                    throw new Exception("Unknown instruction");
            }
        }
    }
}
