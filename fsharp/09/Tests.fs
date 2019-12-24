module Tests

open Xunit
open IntComputer

let RunSingleComputer (state: ComputerState) input earlyStopper =
    let mutable output = 0L
    state.Memory
    |> ExecuteProgram state.InstructionPointer state.RelativeBase (WrapInput input) (fun l -> output <- l) earlyStopper
    |> ignore

    output

[<Fact>]
let Part1() =
    let program = ref (ParseProgram "input.txt")
    array.Resize(program, 10000)

    let result =
        RunSingleComputer
            { ComputerState.Memory = !program
              ComputerState.InstructionPointer = 0
              ComputerState.RelativeBase = 0
              ComputerState.FullStop = false } [ 1L ] None

    Assert.Equal(3280416268L, result)


[<Fact>]
let Part2() =
    let program = ref (ParseProgram "input.txt")
    array.Resize(program, 10000)

    let result =
        RunSingleComputer
            { ComputerState.Memory = !program
              ComputerState.InstructionPointer = 0
              ComputerState.RelativeBase = 0
              ComputerState.FullStop = false } [ 2L ] None

    Assert.Equal(80210L, result)