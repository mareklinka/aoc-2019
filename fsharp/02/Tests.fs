module Tests

open System
open Xunit
open System.Linq

open IntComputer

[<Fact>]
let Part1() =
  let program = System.IO.File.ReadAllText("input.txt").Split(",").Select(fun s -> Int64.Parse(s)).ToArray()
  program.[1] <- 12L
  program.[2] <- 2L
  ExecuteProgram program 0 0 (fun () -> 0L) (fun v -> v |> ignore) |> ignore
  Assert.Equal(3224742L, program.[0])

let rec Find program noun verb target =
  let localProgram = Array.copy program
  localProgram.[1] <- noun
  localProgram.[2] <- verb
  ExecuteProgram localProgram 0 0 (fun () -> 0L) (fun v -> v |> ignore) |> ignore 
  match localProgram.[0] with
  | x when x = target -> noun * 100L + verb
  | _ ->
    match verb with
    | 99L -> Find program (noun+1L) 1L target
    | _   -> Find program noun (verb+1L) target

[<Fact>]
let Part2() =
  let program = System.IO.File.ReadAllText("input.txt").Split(",").Select(fun s -> Int64.Parse(s)).ToArray()
  Assert.Equal(7960L, Find program 1L 1L 19690720L)