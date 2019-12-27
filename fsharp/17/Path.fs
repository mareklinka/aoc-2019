module Path

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