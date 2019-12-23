module Tests

open System
open System.Linq
open Xunit

open IntComputer
open System.IO

[<Fact>]
let Part1() =
    let program = File.ReadAllText("Input.txt").Split(",") |> Array.map int64 

    let outputs = ResizeArray()

    ExecuteProgram program 0 0 (fun () -> 1L) (fun value -> outputs.Add(value) ) |> ignore

    Assert.Equal(8332629L, outputs.Last())

[<Fact>]
let Part2() =
    let program = File.ReadAllText("Input.txt").Split(",") |> Array.map int64 

    let outputs = ResizeArray()

    ExecuteProgram program 0 0 (fun () -> 5L) (fun value -> outputs.Add(value) ) |> ignore

    Assert.Equal(8805067L, outputs.Last())