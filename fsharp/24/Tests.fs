module Tests

open System
open Xunit
open System.Collections.Generic
open System.IO

[<Fact>]
let Part1() =
    let lines = File.ReadAllLines("input.txt")
    let lineLength = lines.[0].Length

    let (_, _, bugs) =
        String.Join("", lines).ToCharArray()
        |> Array.fold (fun (x, y, dict) c ->
            let t =
                match c with
                | '.' -> Empty
                | '#' -> Infested

            dict |> Dictionary.add (x, y) t

            let newX = (x + 1) % lineLength

            let newY =
                match newX with
                | 0 -> y + 1
                | _ -> y

            (newX, newY, dict)) (0, 0, Dictionary<int * int, Tile>())

    let count = Part1.aroundInfested bugs

    let counts =
        bugs
        |> Dictionary.toSeq
        |> Seq.map (fun ((x, y), _) -> (x, y))
        |> Seq.fold (fun dict (x, y) ->
            dict |> Dictionary.add (x, y) (count (x, y))
            dict) (Dictionary<int * int, int>())

    let result = Part1.evolve bugs counts [] Set.empty

    Assert.Equal(32506911, result)

[<Fact>]
let Part2() =
    let lines = File.ReadAllLines("input.txt")
    let lineLength = lines.[0].Length

    let (_, _, bugs) =
        String.Join("", lines).ToCharArray()
        |> Array.fold (fun (x, y, dict) c ->
            let t =
                match c with
                | '.' -> Empty
                | '#' -> Infested

            dict |> Dictionary.add (x, y, 0) t

            let newX = (x + 1) % lineLength

            let newY =
                match newX with
                | 0 -> y + 1
                | _ -> y

            (newX, newY, dict)) (0, 0, Dictionary<int * int * int, Tile>())

    let count = Part2.aroundInfested bugs

    // cartesian product of the three dimensions (level -1, 0, 1)
    let initialCountPositions =
        { -1 .. 1 }
        |> Seq.collect (fun l ->
            { 0 .. 4 }
            |> Seq.collect (fun x ->
                { 0 .. 4}
                |> Seq.map (fun y -> (x, y, l))))


    let counts =
        initialCountPositions
        |> Seq.fold (fun dict (x, y, l) ->
            dict |> Dictionary.add (x, y, l) (count (x, y, l))
            dict) (Dictionary<int * int * int, int>())

    let result = Part2.evolve bugs counts

    Assert.Equal(2025, result)