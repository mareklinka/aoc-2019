module Tests

open System
open IntComputer
open Xunit
open System.IO

type Color =
    | Black = 0
    | White = 1


type Direction =
    | Up
    | Right
    | Down
    | Left

let RunSingleComputer (state: ComputerState) input output =
    state.Memory
    |> ExecuteProgram state.InstructionPointer state.RelativeBase input output None
    |> ignore

    output

let runRobot initialColor =
    let program = ParseProgram "input.txt"
    let mutable currentPosition = (0, 0)
    let mutable paintedPanels = Map.empty |> Map.add currentPosition initialColor

    let inputFunction =
        fun () ->
            let existing = paintedPanels |> Map.tryFind currentPosition
            match existing with
            | Some color -> color
            | None -> Color.Black
            |> int64

    let mutable paintColor = None
    let mutable direction = Up

    let outputFunction =
        fun (value: int64) ->
            match paintColor with
            | None ->
                paintColor <- Some(enum<Color> (value |> int))
                ()
            | Some c ->
                paintedPanels <- paintedPanels |> Map.add currentPosition c
                direction <-
                    match value with
                    | 0L ->
                        match direction with
                        | Up -> Left
                        | Down -> Right
                        | Left -> Down
                        | Right -> Up
                    | 1L ->
                        match direction with
                        | Up -> Right
                        | Down -> Left
                        | Left -> Up
                        | Right -> Down
                currentPosition <-
                    match direction with
                    | Up -> (currentPosition |> fst, (currentPosition |> snd) - 1)
                    | Down -> (currentPosition |> fst, (currentPosition |> snd) + 1)
                    | Right -> ((currentPosition |> fst) + 1, currentPosition |> snd)
                    | Left -> ((currentPosition |> fst) - 1, currentPosition |> snd)

                paintColor <- None

    let computerState =
        { ComputerState.Memory = program
          ComputerState.FullStop = false
          ComputerState.RelativeBase = 0
          ComputerState.InstructionPointer = 0 }

    RunSingleComputer computerState inputFunction outputFunction |> ignore
    paintedPanels

[<Fact>]
let Part1() =
    let panels = runRobot Color.Black

    Assert.Equal(1964, panels |> Map.count)

[<Fact>]
let Part2() =
    let panels = runRobot Color.White

    let minX, maxX, minY, maxY =
        panels
        |> Map.fold
            (fun (minx, maxx, miny, maxy) (x, y) _ ->
                (Math.Min(minx, x), Math.Max(maxx, x), Math.Min(miny, y), Math.Max(maxy, y))) (0, 0, 0, 0)

    let drawing =
        { minY .. maxY }
        |> Seq.fold (fun s row ->
            let newLine =
                { minX .. maxX }
                |> Seq.fold (fun s col ->
                    let newChar =
                        match (panels |> Map.tryFind (col, row)) with
                        | Some Color.White -> "█"
                        | _ -> "░"
                    s + newChar) ""
            s + "\n" + newLine) ""

    File.WriteAllText("output.txt", drawing)
