using System;
using System.Collections.Concurrent;

namespace _15
{
    public sealed class IntComputer
    {
        private long _relativeBase;

        public bool TerminateAfterOutput { get; set; }

        public IntComputer()
        {
            IO = new InputOutput();
        }

        public IntComputer(BlockingCollection<long> input, BlockingCollection<long> output)
        {
            IO = new InputOutput(input, output);
        }

        public IntComputer(IInputOutput io)
        {
            IO = io;
        }

        public IInputOutput IO { get; }

        public long[] Memory { get; private set; }

        private long _ip;

        public long InstructionPointer => _ip;

        public void ExecuteProgram(long[] program, long pointer)
        {
            Memory = program;
            _ip = pointer;

            while (ExecuteInstruction(Memory, ref _ip))
            {
            }
        }

        private bool ExecuteInstruction(long[] program, ref long pointer)
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

                    if (TerminateAfterOutput)
                    {
                        return false;
                    }

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
                case Operation.RelOffset:
                    SetRelativeOffset(program, ref pointer, instruction);
                    break;
                case Operation.Terminate:
                    return false;
            }

            return true;
        }

        private void SetRelativeOffset(long[] program, ref long pointer, long instruction)
        {
            _relativeBase += DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));

            pointer += 2;
        }

        private void IsEqual(long[] program, ref long pointer, long instruction)
        {
            var a = DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = DereferenceParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[GetWritePosition(program[pointer + 3], ReadParameterMode(instruction, 2))] = a == b ? 1 : 0;

            pointer += 4;
        }

        private void LessThan(long[] program, ref long pointer, long instruction)
        {
            var a = DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = DereferenceParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[GetWritePosition (program[pointer + 3], ReadParameterMode(instruction, 2))] = a < b ? 1 : 0;

            pointer += 4;
        }

        private void JumpIfFalse(long[] program, ref long pointer, long instruction)
        {
            var valueToTest = DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var target = DereferenceParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            pointer = valueToTest == 0 ? target : pointer + 3;
        }

        private void JumpIfTrue(long[] program, ref long pointer, long instruction)
        {
            var valueToTest = DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var target = DereferenceParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            pointer = valueToTest != 0 ? target : pointer + 3;
        }

        private void Write(long[] program, ref long pointer, long instruction, IInputOutput io)
        {
            var value = DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));

            io.Write(value);

            pointer += 2;
        }

        private void Read(long[] program, ref long pointer, IInputOutput io)
        {
            program[GetWritePosition(program[pointer + 1], ReadParameterMode(program[pointer], 0))] = io.Read();
            pointer += 2;
        }

        private void Multiply(long[] program, ref long pointer, long instruction)
        {
            var a = DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = DereferenceParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[GetWritePosition(program[pointer + 3], ReadParameterMode(program[pointer], 2))] = a * b;
            pointer += 4;
        }

        private void Add(long[] program, ref long pointer, long instruction)
        {
            var a = DereferenceParameter(program, program[pointer + 1], ReadParameterMode(instruction, 0));
            var b = DereferenceParameter(program, program[pointer + 2], ReadParameterMode(instruction, 1));

            program[GetWritePosition(program[pointer + 3], ReadParameterMode(program[pointer], 2))] = a + b;
            pointer += 4;
        }

        private static ParameterMode ReadParameterMode(long instruction, int position)
        {
            instruction /= (int)Math.Pow(10, 2 + position);
            instruction %= 10;

            return (ParameterMode)instruction;
        }

        private long DereferenceParameter(long[] program, long parameterValue, ParameterMode mode)
        {
            return mode switch
            {
                ParameterMode.Address => program[parameterValue],
                ParameterMode.Immediate => parameterValue,
                ParameterMode.Relative => program[_relativeBase + parameterValue],
                _ => throw new Exception()
            };
        }

        private long GetWritePosition(long parameterValue, ParameterMode mode)
        {
            return mode switch
            {
                ParameterMode.Address => parameterValue,
                ParameterMode.Immediate => throw new Exception(),
                ParameterMode.Relative => _relativeBase + parameterValue,
                _ => throw new Exception()
            };
        }

        public interface IInputOutput
        {
            long Read();
            void Write(long value);
        }

        public class InputOutput : IInputOutput
        {
            // underlying concurrent queue will ensure a FIFO execution order
            public BlockingCollection<long> Input { get; }
            public BlockingCollection<long> Output { get; }

            public InputOutput()
            {
                Input = new BlockingCollection<long>(new ConcurrentQueue<long>());
                Output = new BlockingCollection<long>(new ConcurrentQueue<long>());
            }

            public InputOutput(BlockingCollection<long> input, BlockingCollection<long> output)
            {
                Input = input;
                Output = output;
            }

            public void AddInput(long value)
            {
                Input.Add(value);
            }

            public long Read()
            {
                return Input.Take();
            }

            public long ReadOutput()
            {
                return Output.Take();
            }

            public void Write(long value)
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
            RelOffset = 9,
            Terminate = 99
        }

        private enum ParameterMode
        {
            Address = 0,
            Immediate = 1,
            Relative = 2
        }
    }
}
