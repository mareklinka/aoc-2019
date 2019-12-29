// Learn more about F# at http://fsharp.org

open System
open IntComputer

let bufferedOutput =
    let buffer = ResizeArray()
    let writer value =
        match value with
        | 10L ->
            let message = new string(buffer |> Seq.map (fun l -> l |> char) |> Array.ofSeq)
            Console.WriteLine(message)
            buffer.Clear()
        | _ ->
            buffer.Add(value)
    writer

let consoleInput =
    let mutable str = ""
    let rec reader() =
        match str with
        | "" ->
           str <- Console.ReadLine() + "\n"
           reader()
        | _ ->
            let c = str.[0]
            str <- str.Substring(1)
            c |> int64
    reader

[<EntryPoint>]
let main argv =
    Console.WriteLine("Items you need to pass the checkpoint: ")
    Console.WriteLine("space law space brochure, space heater, hologram, spool of cat 6")

    let program = parseProgram "input.txt"

    program |> ExecuteProgram 0 0 (consoleInput) (bufferedOutput) None |> ignore

    Console.WriteLine()
    Console.WriteLine()
    Console.WriteLine("Game finished - press any key to continue")
    Console.ReadLine() |> ignore
    0 // return an integer exit code
