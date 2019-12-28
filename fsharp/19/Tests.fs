module Tests

open Xunit

open IntComputer

let computePoint program (x, y) =
    match ((x < 0L) || (y < 0L)) with
    | true -> 0L
    | false ->
        let mutable state = 0L
        program
        |> Array.copy
        |> ExecuteProgram 0 0 (WrapInput [ x; y ]) (fun value -> state <- value) None
        |> ignore
        state

let rec fitObject size program (x, y) =
    let rec edge target (x, y)  =
        let state = computePoint program (x, y)
        match state with
        | v when v <> target -> edge target (x + 1L, y)
        | _ -> x

    let firstInBeam = edge 1L (x,y)
    let firstOutOfBeam = edge 0L (firstInBeam, y)
    let topLeft = computePoint program (firstOutOfBeam - size, y)
    match topLeft with
    | 0L -> fitObject size program (firstOutOfBeam - 5L, y + 1L)
    | 1L ->
        let bottomLeft = computePoint program (firstOutOfBeam - size, y + size - 1L)
        match bottomLeft with
        | 0L -> fitObject size program (firstOutOfBeam - 5L, y + 1L)
        | 1L -> (firstOutOfBeam - size) * 10000L + y

[<Fact>]
let Part1() =
    let program = parseProgram "input.txt"
    let mutable count = 0L

    { 0L .. 49L }
    |> Seq.iter (fun row ->
        { 0L .. 49L }
        |> Seq.iter (fun col ->
            program
            |> Array.copy
            |> ExecuteProgram 0 0 (WrapInput [ col; row ]) (fun value -> count <- count + value) None
            |> ignore))

    Assert.Equal(114L, count)

[<Fact>]
let Part2() =
    let program = parseProgram "input.txt"

    let result = fitObject 100L program (0L, 100L)

    Assert.Equal(10671712L, result)