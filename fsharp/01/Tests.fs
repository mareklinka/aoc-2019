module Tests

open Xunit
open System.IO

let rec ComputeFuel (weight:int64) =
    let fuelWeight = weight / 3L - 2L
    match fuelWeight with
    | w when w <= 0L -> 0L
    | _ -> fuelWeight + ComputeFuel(fuelWeight)

[<Fact>]
let ``Part 1`` () =
    let part1 = File.ReadAllLines("input.txt") |> Array.sumBy(fun line -> System.Int64.Parse(line) / 3L - 2L)
    Assert.Equal(3159380L, part1)

[<Fact>]
let ``Part 2`` () =
    let part1 = File.ReadAllLines("input.txt") |> Array.sumBy(fun line -> ComputeFuel(System.Int64.Parse(line)))
    Assert.Equal(4736213L, part1)
    
