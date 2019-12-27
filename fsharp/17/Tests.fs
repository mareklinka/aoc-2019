module Tests

open System
open Xunit

open IntComputer

[<Fact>]
let Part1() =
    let program = parseProgram "input.txt"
    let area, _ = program |> Path.getArea

    let scaffoldLocations =
        area
        |> Map.toSeq
        |> Seq.filter (fun (_, t) -> t = Path.Tile.Scaffold)
        |> Seq.map fst

    let intersections =
        scaffoldLocations
        |> Seq.fold (fun acc (x, y) ->
            let aroundCount =
                Path.around (x, y)
                |> Seq.fold (fun acc (x, y) ->
                    let tile = area |> Map.tryFind (x, y)
                    match tile with
                    | Some(Path.Tile.Scaffold) -> acc + 1
                    | _ -> acc) 0

            match aroundCount with
            | 4 -> acc + (x * y)
            | _ -> acc) 0

    Assert.Equal(6000, intersections)

let asciiEncode (list: string list) =
    String.Join(",", list).ToCharArray()
    |> Array.map (fun c -> c |> int64)
    |> List.ofArray

[<Fact>]
let Part2() =
    let program = parseProgram "input.txt"

    let area, robot =
        program
        |> Array.copy
        |> Path.getArea

    let movement = Path.constructPath area robot Path.Direction.North None 0 []

    // now we need to split the movement instructions into three sets so that their combination covers the whole path
    let x = Decomposition.findSubgroups movement
    let _, codes = Decomposition.verify movement x.[0] x.[1] x.[2] []

    let mainRoutine = codes |> asciiEncode

    let routineA =
        x.[0]
        |> List.map (fun i -> i.Code)
        |> asciiEncode

    let routineB =
        x.[1]
        |> List.map (fun i -> i.Code)
        |> asciiEncode

    let routineC =
        x.[2]
        |> List.map (fun i -> i.Code)
        |> asciiEncode

    let navigationProgram =
        mainRoutine
        @ [ 10L ] @ routineA @ [ 10L ] @ routineB @ [ 10L ] @ routineC @ [ 10L ] @ [ 'n' |> int64 ] @ [ 10L ]

    program.[0] <- 2L

    let mutable dustGathered = 0L

    program
    |> ExecuteProgram 0 0 (WrapInput navigationProgram) (fun value -> dustGathered <- value) None
    |> ignore

    Assert.Equal(807320L, dustGathered)