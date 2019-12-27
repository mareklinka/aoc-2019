module Tests

open System
open Xunit

open IntComputer

type Tile =
    | Scaffold = 35
    | Empty = 46

type Direction =
    | North
    | East
    | South
    | West

type TurnDirection =
    | Left
    | Right

type Instruction =
    { Direction: TurnDirection
      Distance: int }

    member __.Length = 1 + (if __.Distance > 9 then 2 else 1)
    member __.Code =
        let d =
            match __.Direction with
            | Left -> "L,"
            | Right -> "R,"
        (d + (__.Distance |> string))

let move (x, y) direction =
    match direction with
    | North -> (x, y - 1)
    | East -> (x + 1, y)
    | South -> (x, y + 1)
    | West -> (x - 1, y)

let changeDirection turnDirection walkDirection =
    match turnDirection with
    | Left ->
        match walkDirection with
        | North -> West
        | East -> North
        | West -> South
        | South -> East
    | Right ->
        match walkDirection with
        | North -> East
        | East -> South
        | West -> North
        | South -> West

let turn area (x, y) direction =
    let (a, b, c, d) =
        match direction with
        | North -> (x - 1, y, x + 1, y)
        | South -> (x + 1, y, x - 1, y)
        | East -> (x, y - 1, x, y + 1)
        | West -> (x, y + 1, x, y - 1)

    let leftAttempt = area |> Map.tryFind (a, b)
    match leftAttempt with
    | Some(Tile.Scaffold) -> Some(Left, (a, b))
    | _ ->
        let rightAttempt = area |> Map.tryFind (c, d)
        match rightAttempt with
        | Some(Tile.Scaffold) -> Some(Right, (c, d))
        | _ -> None

let rec constructPath area (x, y) direction lastTurn step acc =
    let nextPos = move (x, y) direction
    let tile = area |> Map.tryFind nextPos

    match tile with
    | Some(Tile.Scaffold) -> constructPath area nextPos direction lastTurn (step + 1) acc
    | _ ->
        let newAcc =
            match lastTurn with
            | Some(d) ->
                let instruction =
                    { Instruction.Direction = d
                      Instruction.Distance = step }
                acc @ [ instruction ]
            | _ -> acc

        let nextTurn = turn area (x, y) direction
        match nextTurn with
        | Some((turn, p)) -> constructPath area p (changeDirection turn direction) (Some(turn)) 1 newAcc
        | None ->
            match lastTurn with
            | Some(t) ->
                acc @ [ { Instruction.Direction = t
                          Instruction.Distance = step } ]
            | _ -> failwith "Ehm, are you sure?"


let getArea program =
    let mutable area = Map.empty
    let mutable point = (0, 0)
    let mutable robot = (0, 0)

    program
    |> ExecuteProgram 0 0 (fun () -> -1L) (fun value ->
           match value with
           | 10L -> point <- (0, (point |> snd) + 1)
           | 35L
           | 46L ->
               area <- area |> Map.add point (enum<Tile> (value |> int))
               point <- ((point |> fst) + 1, point |> snd)
           | _ ->
               robot <- point
               point <- ((point |> fst) + 1, point |> snd)) None
    |> ignore

    area, robot

let around (x, y) =
    seq {
        yield (x - 1, y)
        yield (x + 1, y)
        yield (x, y - 1)
        yield (x, y + 1)
    }

let verifyLength (part: Instruction list) =
    let length = (part |> List.sumBy (fun i -> i.Length)) + (part |> List.length)
    length <= 15

let rec verify full part1 part2 part3
        codes =
    let correctLengths = (part1 |> verifyLength) && (part2 |> verifyLength) && (part3 |> verifyLength)

    match correctLengths with
    | false -> false, []
    | true ->
        match full with
        | [] -> true, codes
        | _ ->
            let parts =
                seq {
                    part1
                    part2
                    part3 }

            let matchingPart =
                parts
                |> Seq.filter (fun p ->
                    let l = full.Length >= p.Length
                    let x = full |> List.take (p |> List.length)
                    let y = x |> List.zip p
                    let z = y |> List.forall (fun (a, b) -> a = b)
                    l && z)
                |> Seq.tryExactlyOne

            match matchingPart with
            | Some(part) ->
                let index = parts |> Seq.findIndex (fun p -> p = part)

                let code =
                    match index with
                    | 0 -> "A"
                    | 1 -> "B"
                    | 2 -> "C"
                verify (full |> List.skip part.Length) part1 part2 part3 (codes @ [ code ])
            | None -> false, []

let rec remove haystack needle acc =
    // this is a horrible way to go around it but.. who has time to implement Knuth-Morris-Pratt?
    // basically, I just compare the start of the sequence to the needle
    // if they match, I don't put anything into the accumulator and move "length of needle" characters forward
    // if they don't match, I put the first item in the accumulator and call recursively on tail
    let enough = (haystack |> List.length) >= (needle |> List.length)
    match enough with
    | true ->
        match haystack with
        | i :: tail ->
            let equal =
                haystack
                |> List.take needle.Length
                |> List.zip needle
                |> List.forall (fun (a, b) -> a = b)
            match equal with
            | true -> remove (haystack |> List.skip needle.Length) needle acc
            | false -> remove tail needle (acc @ [ i ])
        | _ -> failwith "How?"
    | false -> acc @ haystack

let findSubgroups instructions =
    { 1 .. (instructions |> List.length) - 2 } // all possible lengths for part 1
    |> Seq.fold (fun acc c1 ->
        match acc with
        | _ :: _ -> acc // short-circuit
        | _ ->
            let part1 = instructions |> List.take c1
            let r1 =
                remove instructions part1 [] // this removes all the occurences of part1 from the original sequence

            { 1 .. (r1 |> List.length) - 1 } // all possible lengths for part 2
            |> Seq.fold (fun acc c2 ->
                match acc with
                | _ :: _ -> acc // short-circuit
                | _ ->
                    let part2 = r1 |> List.take c2
                    let r2 = remove r1 part2 [] // this also removes all occurences of part 2

                    { 1 .. (r2 |> List.length) } // all possible lengths for part 3
                    |> Seq.fold (fun acc c3 ->
                        match acc with
                        | _ :: _ -> acc // short-circuit
                        | _ ->
                            let part3 =
                                r2
                                |> List.take
                                    c3 // part 3 is drawn from the remainder, after part1 and part2 have been removed

                            // now we have all three parts - let's verify if they decompose the instruction list
                            let (verified, _) = verify instructions part1 part2 part3 []
                            match verified with
                            | true ->
                                // the only branch that fills the accumulator
                                // this will get immediately propagated out due to short-circuiting the folds
                                [ part1; part2; part3 ]
                            | false -> acc) []) []) []

[<Fact>]
let Part1() =
    let program = parseProgram "input.txt"
    let area, _ = program |> getArea

    let scaffoldLocations =
        area
        |> Map.toSeq
        |> Seq.filter (fun (_, t) -> t = Tile.Scaffold)
        |> Seq.map fst

    let intersections =
        scaffoldLocations
        |> Seq.fold (fun acc (x, y) ->
            let aroundCount =
                around (x, y)
                |> Seq.fold (fun acc (x, y) ->
                    let tile = area |> Map.tryFind (x, y)
                    match tile with
                    | Some(Tile.Scaffold) -> acc + 1
                    | _ -> acc) 0

            match aroundCount with
            | 4 -> acc + (x * y)
            | _ -> acc) 0

    Assert.Equal(6000, intersections)

let asciiEncode (list: string list) =
    String.Join(",", list).ToCharArray()
    |> Array.map (fun c -> c |> int64)
    |> List.ofArray

[<Fact>]
let Part2() =
    let program = parseProgram "input.txt"

    let area, robot =
        program
        |> Array.copy
        |> getArea

    let movement = constructPath area robot North None 0 []

    // now we need to split the movement instructions into three sets so that their combination covers the whole path
    let x = findSubgroups movement
    let _, codes = verify movement x.[0] x.[1] x.[2] []

    let mainRoutine = codes |> asciiEncode

    let routineA =
        x.[0]
        |> List.map (fun i -> i.Code)
        |> asciiEncode

    let routineB =
        x.[1]
        |> List.map (fun i -> i.Code)
        |> asciiEncode

    let routineC =
        x.[2]
        |> List.map (fun i -> i.Code)
        |> asciiEncode

    let navigationProgram =
        mainRoutine
        @ [ 10L ] @ routineA @ [ 10L ] @ routineB @ [ 10L ] @ routineC @ [ 10L ] @ [ 'n' |> int64 ] @ [ 10L ]

    program.[0] <- 2L

    let mutable dustGathered = 0L

    program
    |> ExecuteProgram 0 0 (WrapInput navigationProgram) (fun value -> dustGathered <- value) None
    |> ignore

    Assert.Equal(807320L, dustGathered)