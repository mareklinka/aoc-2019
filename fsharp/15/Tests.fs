module Tests

open IntComputer
open System
open Xunit

let rec findLongestRoute states visited =
    match states with
    | [] ->
        visited
        |> Map.toSeq
        |> Seq.map (fun (_, s) -> s)
        |> Seq.max
    | (state, (x, y), steps) :: tail ->
        let aroundStates =
            { 1L .. 4L }
            |> Seq.fold (fun acc dir ->
                let mutable output = -1L
                let programCopy = state.Memory |> Array.copy
                let newState =
                    ExecuteProgram state.InstructionPointer state.RelativeBase (fun () -> dir)
                        (fun value -> output <- value) (Some(fun i -> i = 4L)) programCopy

                let newPosition =
                    match dir with
                    | 1L -> (x, y - 1)
                    | 2L -> (x, y + 1)
                    | 3L -> (x - 1, y)
                    | 4L -> (x + 1, y)
                acc |> List.append [ (newState, newPosition, output) ]) []

        let nextPositions =
            aroundStates
            |> List.filter (fun (_, p, o) -> (o = 1L) && (not (visited |> Map.containsKey p)))
            |> List.map (fun (cs, (a, b), _) -> (cs, (a, b), steps + 1))

        let newVisited = visited |> Map.add (x, y) steps

        let newStates = tail @ nextPositions

        findLongestRoute newStates newVisited

let rec findOxygenTank states visited =
    match states with
    | [] -> failwith "I got lost"
    | (state, (x, y), steps) :: tail ->
        let aroundStates =
            { 1L .. 4L }
            |> Seq.fold (fun acc dir ->
                let mutable output = -1L
                let programCopy = state.Memory |> Array.copy
                let newState =
                    ExecuteProgram state.InstructionPointer state.RelativeBase (fun () -> dir)
                        (fun value -> output <- value) (Some(fun i -> i = 4L)) programCopy

                let newPosition =
                    match dir with
                    | 1L -> (x, y - 1)
                    | 2L -> (x, y + 1)
                    | 3L -> (x - 1, y)
                    | 4L -> (x + 1, y)
                acc |> List.append [ (newState, newPosition, output) ]) []

        let foundOxygen = aroundStates |> List.tryFind (fun (_, _, r) -> r = 2L)

        match foundOxygen with
        | Some(cs, pos, _) -> (cs, steps + 1, pos)
        | None ->
            let nextPositions =
                aroundStates
                |> List.filter (fun (_, p, o) -> (o <> 0L) && (not (visited |> Set.contains p)))
                |> List.map (fun (cs, (a, b), _) -> (cs, (a, b), steps + 1))

            let newVisited = visited |> Set.add (x, y)

            let newStates = tail @ nextPositions
            findOxygenTank newStates newVisited

[<Fact>]
let Part1() =
    let program = parseProgram "input.txt"

    let initialState =
        { ComputerState.Memory = program
          ComputerState.InstructionPointer = 0
          ComputerState.RelativeBase = 0
          ComputerState.FullStop = false }

    let (_, d, _) = findOxygenTank [ initialState, (0, 0), 0 ] Set.empty

    Assert.Equal(212, d)

[<Fact>]
let Part2() =
    let program = parseProgram "input.txt"

    let initialState =
        { ComputerState.Memory = program
          ComputerState.InstructionPointer = 0
          ComputerState.RelativeBase = 0
          ComputerState.FullStop = false }

    let (cs, _, (x, y)) = findOxygenTank [ initialState, (0, 0), 0 ] Set.empty

    let result = findLongestRoute [ (cs, (x, y), 0) ] Map.empty

    Assert.Equal(358, result)
