module Tests

open Xunit
open IntComputer

let rec permutations (digits: int list) =
    match digits with
    | [ last ] -> [ [ last ] ]
    | _ ->
        digits
        |> List.fold (fun acc digit ->
            let successor =
                digits
                |> List.except [ digit ]
                |> permutations // permutations of a shorter length
            acc |> List.append (successor |> List.map (fun suc -> List.append [ digit ] suc))) ([])

let WrapInput(list: int64 list) =
    let mutable state = -1
    fun () ->
        state <- state + 1
        list.[state]

let RunSingleComputer (state: ComputerState) input output earlyStopper =
    let inputList = [ !output ] |> List.append input
    let newState =
        state.Memory
        |> ExecuteProgram state.InstructionPointer state.RelativeBase (WrapInput inputList) (fun l -> output := l)
               earlyStopper

    newState

let RunComputers program (phases: int list) =
    let lastOutput = ref 0L

    [ 0 .. 4 ]
    |> Seq.iter (fun i ->
        RunSingleComputer
            { ComputerState.Memory = program |> Array.copy
              ComputerState.InstructionPointer = 0
              ComputerState.RelativeBase = 0
              ComputerState.FullStop = false } ([ phases.[i] |> int64 ]) lastOutput None
        |> ignore)

    !lastOutput

let rec RunFeedbackLoop (states: ComputerState list) firstPass input (phases: int list) =
    let earlyStopper = (Some(fun i -> i = 4L))
    let output = ref input

    let inputFunction =
        (fun i ->
            match firstPass with
            | true -> [ phases.[i] |> int64 ]
            | false -> [])

    let newStates =
        [ 0 .. 4 ] |> List.map (fun i -> RunSingleComputer states.[i] (inputFunction (i)) output earlyStopper)
    let allFinished = newStates |> List.forall (fun s -> s.FullStop)

    match allFinished with
    | true -> !output
    | false -> RunFeedbackLoop newStates false !output phases

[<Fact>]
let Part1() =
    let program = ParseProgram "input.txt"

    let perm = [ 0 .. 4 ] |> permutations
    Assert.Equal(5 * 4 * 3 * 2, List.length (perm))

    let maxThrust =
        perm
        |> List.map (fun p -> p |> RunComputers program)
        |> Seq.max

    Assert.Equal(17440L, maxThrust)

[<Fact>]
let Part2() =
    let program = ParseProgram "input.txt"

    let perm = [ 5 .. 9 ] |> permutations

    let states =
        [ 0 .. 4 ]
        |> Seq.map (fun _ ->
            { ComputerState.Memory = program |> Array.copy
              ComputerState.InstructionPointer = 0
              ComputerState.RelativeBase = 0
              ComputerState.FullStop = false })
        |> List.ofSeq

    let maxThrust =
        perm
        |> List.map (fun p -> p |> RunFeedbackLoop states true 0L)
        |> Seq.max

    Assert.Equal(27561242L, maxThrust)
)