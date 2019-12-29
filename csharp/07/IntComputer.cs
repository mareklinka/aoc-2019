using System;
using System.Collections.Concurrent;

namespace _07
{
    public sealed class IntComputer
    {
        public IntComputer()
        {
            IO = new InputOutput();
        }

        public IntComputer(BlockingCollection<int> input, BlockingCollection<int> output)
        {
            IO = new InputOutput(input, output);
        }

        public InputOutput IO { get; }

        public void ExecuteProgram(int[] program)
        {
            var pointer = 0;

            while (ExecuteInstruction(program, ref pointer))
            {
            }
        }

        private bool ExecuteInstruction(int[] program, ref int pointer)
        {
            var instruction = program[pointer];
            var operation = (Operation)(instruction % 100);

            switch (operation)
            {
                case Operation.Add:
                    Add(program, ref pointer, instruction);
                    break;
                case Operation.Multiply:
                    Multiply(program, ref pointer, instruction);
                    break;
                case Operation.Read:
                    Read(program, ref pointer, IO);
                    break;
                case Operation.Write:
                    Write(program, ref pointer, instruction, IO);
                    break;
                case Operation.JFT:
                    JumpIfTrue(program, ref pointer, instruction);
                    break;
                case Operation.JFF:
                    JumpIfFalse(program, ref pointer, instruction);
                    break;
                case Operation.LessThan:
                    LessThan(program, ref pointer, instruction);
                    break;
                case Operation.Equals:
                    IsEqual(program, ref pointer, instruction);
                    break;
                case Operation.Terminate:
                    return false;
            }

            return true;
        }

        private static void IsEqual(int[] program, ref int pointer, int instruction)
        {
            var a = ReadParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = ReadParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[ReadParameter(program, program[pointer + 3], ParameterMode.Immediate)] = a == b ? 1 : 0;

            pointer += 4;
        }

        private static void LessThan(int[] program, ref int pointer, int instruction)
        {
            var a = ReadParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = ReadParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[ReadParameter(program, program[pointer + 3], ParameterMode.Immediate)] = a < b ? 1 : 0;

            pointer += 4;
        }

        private static void JumpIfFalse(int[] program, ref int pointer, int instruction)
        {
            var valueToTest = ReadParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var target = ReadParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            pointer = valueToTest == 0 ? target : pointer + 3;
        }

        private static void JumpIfTrue(int[] program, ref int pointer, int instruction)
        {
            var valueToTest = ReadParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var target = ReadParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            pointer = valueToTest != 0 ? target : pointer + 3;
        }

        private static void Write(int[] program, ref int pointer, int instruction, InputOutput io)
        {
            var value = ReadParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));

            io.Write(value);

            pointer += 2;
        }

        private static void Read(int[] program, ref int pointer, InputOutput io)
        {
            program[ReadParameter(program, program[pointer + 1], ParameterMode.Immediate)] = io.Read();
            pointer += 2;
        }

        private static void Multiply(int[] program, ref int pointer, int instruction)
        {
            var a = ReadParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = ReadParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[ReadParameter(program, program[pointer + 3], ParameterMode.Immediate)] = a * b;
            pointer += 4;
        }

        private static void Add(int[] program, ref int pointer, int instruction)
        {
            var a = ReadParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = ReadParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[ReadParameter(program, program[pointer + 3], ParameterMode.Immediate)] = a + b;
            pointer += 4;
        }

        private static ParameterMode ReadParameterMode(int instruction, int position)
        {
            instruction /= (int)Math.Pow(10, 2 + position);
            instruction %= 10;

            return (ParameterMode)instruction;
        }

        private static int ReadParameter(int[] program, int address, ParameterMode mode)
        {
            return mode switch
            {
                ParameterMode.Address => program[address],
                ParameterMode.Immediate => address,
                _ => throw new Exception()
            };
        }

        public class InputOutput
        {
            // underlying concurrent queue will ensure a FIFO execution order
            public BlockingCollection<int> Input { get; }
            public BlockingCollection<int> Output { get; }

            public InputOutput()
            {
                Input = new BlockingCollection<int>(new ConcurrentQueue<int>());
                Output = new BlockingCollection<int>(new ConcurrentQueue<int>());
            }

            public InputOutput(BlockingCollection<int> input, BlockingCollection<int> output)
            {
                Input = input;
                Output = output;
            }

            public void AddInput(int value)
            {
                Input.Add(value);
            }

            public int Read()
            {
                Input.TryTake(out var value, TimeSpan.FromMilliseconds(100));

                return value;
            }

            public int ReadOutput()
            {
                Output.TryTake(out var value, TimeSpan.FromMilliseconds(100));

                return value;
            }

            public void Write(int value)
            {
                Output.Add(value);
            }
        }

        private enum Operation
        {
            Add = 1,
            Multiply = 2,
            Read = 3,
            Write = 4,
            JFT = 5,
            JFF = 6,
            LessThan = 7,
            Equals = 8,
            Terminate = 99
        }

        private enum ParameterMode
        {
            Address = 0,
            Immediate = 1
        }
    }
}
