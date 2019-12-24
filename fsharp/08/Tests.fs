module Tests

open System
open Xunit
open System.IO
open System.Text

[<Fact>]
let Part1() =
    let size = 25 * 6
    let input = File.ReadAllText("input.txt").ToCharArray()

    let (_, _, oneCount, twoCount) =
        { 0 .. (input.Length / size - 1) }
        |> Seq.fold (fun (layer, zc, oc, tc) offset ->
            let l =
                input
                |> Array.skip (offset * size)
                |> Array.take size

            let z, o, t =
                l
                |> Array.fold (fun (z, o, t) c ->
                    match c with
                    | '0' -> (z + 1, o, t)
                    | '1' -> (z, o + 1, t)
                    | '2' -> (z, o, t + 1)
                    | _ -> (z, o, t)) (0, 0, 0)

            match z < zc with
            | true -> (l, z, o, t)
            | false -> (layer, zc, oc, tc)) (Array.empty, 100000, 0, 0)

    Assert.Equal(1320, oneCount * twoCount)

[<Fact>]
let Part2() =
    let size = 25 * 6
    let input = File.ReadAllText("input.txt").ToCharArray()

    let finalPicture =
        seq {
            for i in 1 .. size do
                yield '2'
        }
        |> Array.ofSeq

    let layer =
        { 0 .. (input.Length / size - 1) }
        |> Seq.fold (fun (acc: char array) offset ->
            let l =
                input
                |> Array.skip (offset * size)
                |> Array.take size

            l
            |> Array.iteri (fun i c ->
                match acc.[i] with
                | '2' -> acc.[i] <- c
                | _ -> ())
            acc) (finalPicture)

    let stringBuilder = StringBuilder()

    layer
    |> Array.iteri (fun i c ->
        let toDraw =
            match c with
            | '0' -> '░'
            | '1' -> '█'
            | '2' -> ' '

        match i % 25 with
        | 0 ->
            stringBuilder.AppendLine() |> ignore
            stringBuilder.Append(toDraw) |> ignore
        | _ -> stringBuilder.Append(toDraw) |> ignore)

    File.WriteAllText("output.txt", stringBuilder.ToString())
 