module Tests

open System
open Xunit
open System.IO

let rec createPattern orig i =
    seq {
        yield! orig |> Seq.collect (fun o -> Seq.replicate i o)
        yield! createPattern orig i
    }

let toInt count array =
    array
    |> Array.take count
    |> Array.fold (fun acc c -> acc * 10 + c) 0

[<Fact>]
let Part1() =
    let input = File.ReadAllText("input.txt").ToCharArray() |> Array.map (fun c -> (c |> int) - ('0' |> int))

    let patternSource =
        seq {
            0
            1
            0
            -1 }

    let result =
        { 1 .. 100 }
        |> Seq.fold (fun acc _ ->
            acc
            |> Array.mapi (fun i _ ->
                let pattern = createPattern patternSource (i + 1) |> Seq.skip 1

                let newVal =
                    acc
                    |> Seq.zip pattern
                    |> Seq.sumBy (fun (a, b) -> a * b)

                (Math.Abs newVal) % 10)) (input)

    Assert.Equal
        (73127523, result |> toInt 8)

[<Fact>]
let Part2() =
    let input = File.ReadAllText("input.txt").ToCharArray() |> Array.map (fun c -> (c |> int) - ('0' |> int))

    let input =
        seq { 1 .. 10000 }
        |> Seq.collect (fun _ -> input)
        |> Array.ofSeq

    let offset = input |> toInt 7

    let inputTail = input |> Array.skip offset

    let result =
        { 1 .. 100 }
        |> Seq.fold (fun acc _ ->
            let mutable s =
                acc
                |> Array.sum
                |> int
                |> Math.Abs

            acc
            |> Array.map (fun current ->
                let newVal = s % 10
                s <- s - current
                newVal)) inputTail

    Assert.Equal(80284420, result |> toInt 8)