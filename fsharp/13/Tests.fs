module Tests

open System
open Xunit

open IntComputer

[<Fact>]
let Part1() =
    let program = ParseProgram "input.txt"

    let buffer = ResizeArray()
    let mutable blockCount = 0

    program
    |> ExecuteProgram 0 0 (fun () -> 0L) (fun l ->
           buffer.Add l
           match buffer.Count with
           | 3 ->
               match l with
               | 2L -> blockCount <- blockCount + 1
               | _ -> ()
               buffer.Clear()
           | _ -> ()) None
    |> ignore

    Assert.Equal(193, blockCount)

[<Fact>]
let Part2() =
    let program = ParseProgram "input.txt"
    program.[0] <- 2L

    let buffer = ResizeArray()
    let mutable paddleX = 0L
    let mutable ballX = 0L
    let mutable score = 0L

    let reader = fun () -> ballX.CompareTo(paddleX) |> int64

    let writer =
        fun value ->
            match buffer.Count with
            | 2 ->
                match value with
                | 3L -> paddleX <- buffer.[0]
                | 4L -> ballX <- buffer.[0]
                | _ ->
                    match (buffer.[0], buffer.[1]) with
                    | (-1L, 0L) -> score <- value
                    | _ -> ()

                buffer.Clear()
            | _ -> buffer.Add value

    program
    |> ExecuteProgram 0 0 reader writer None
    |> ignore

    Assert.Equal(10547L, score)
