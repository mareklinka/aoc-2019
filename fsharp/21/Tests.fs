module Tests

open Xunit

open IntComputer

let asciiEncode (script: string) =
    script.ToCharArray()
    |> Array.map (fun c -> c |> int64)
    |> List.ofArray

[<Fact>]
let Part1() =
    let program = parseProgram "input.txt"
    let script = "NOT A T\nNOT B J\nOR T J\nNOT C T\nOR T J\nAND D J\nWALK\n"
    let input = script |> asciiEncode |> WrapInput
    let mutable output = -1L

    program |> ExecuteProgram 0 0 input (fun value -> output <- max output value) None |> ignore

    Assert.Equal(19354464L, output)

[<Fact>]
let Part2() =
    let program = parseProgram "input.txt"
    let script = "NOT A T\nNOT B J\nOR T J\nNOT C T\nOR T J\nNOT E T\nNOT T T\nAND I T\nOR H T\nAND T J\nNOT F T\nNOT T T\nAND E T\nOR T J\nAND D J\nRUN\n"
    let input = script |> asciiEncode |> WrapInput
    let mutable output = -1L

    program |> ExecuteProgram 0 0 input (fun value -> output <- max output value) None |> ignore

    Assert.Equal(1143198454L, output)