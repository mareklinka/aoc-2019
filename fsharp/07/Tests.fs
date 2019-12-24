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

let RunSingleComputer (state: ComputerState) phase input output earlyStopper =
    match phase with
    | Some p ->
        let newState =
            state.Memory
            |> ExecuteProgram state.InstructionPointer state.RelativeBase
                   (WrapInput
                       [ (p |> int64)
                         input ]) (fun l -> output := l) earlyStopper
        newState
    | None ->
        let newState =
            state.Memory
            |> ExecuteProgram state.InstructionPointer state.RelativeBase (WrapInput [ input ])
                   (fun l -> output := l) earlyStopper
        newState

let RunComputers program (phases: int list) =
    let lastOutput = ref 0L

    [ 0 .. 4 ]
    |> Seq.iter (fun i ->
        RunSingleComputer
            { ComputerState.Memory = program |> Array.copy
              ComputerState.InstructionPointer = 0
              ComputerState.RelativeBase = 0
              ComputerState.FullStop = false }
              (Some(phases.[i]))
              !lastOutput
              lastOutput
              None
              |> ignore)

    !lastOutput

let rec RunFeedbackLoop (states: ComputerState list) firstPass input (phases: int list) =
    let earlyStopper = (Some(fun i -> i = 4L))
    let output = ref input

    match firstPass with
    | true ->
        let newStates = [0 .. 4] |> List.map (fun i -> RunSingleComputer states.[i] (Some(phases.[i])) !output output earlyStopper)

        RunFeedbackLoop newStates false !output phases
    | false ->
        let newStates = [0 .. 4] |> List.map (fun i -> RunSingleComputer states.[i] None !output output earlyStopper)
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