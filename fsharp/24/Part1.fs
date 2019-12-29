[<RequireQualifiedAccess>]
module Part1

let score bugs =
    bugs
    |> Dictionary.keys
    |> Seq.sumBy (fun (x, y) ->
        match bugs |> Dictionary.find (x, y) with
        | Infested -> pown 2 (y * 5 + x)
        | Empty -> 0)

let around (x, y) =
    seq {
        (x - 1, y)
        (x + 1, y)
        (x, y - 1)
        (x, y + 1)
    }
    |> Seq.filter (fun (x, y) -> (x >= 0) && (x < 5) && (y >= 0) && (y < 5))

let aroundInfested map (x, y) =
    (x, y)
    |> around
    |> Seq.sumBy (fun (x, y) ->
        match map |> Dictionary.tryFind (x, y) with
        | Some(Infested) -> 1
        | _ -> 0)

let rec evolve bugs neighbors changes history =
    changes
    |> List.iter (fun (x, y, c) -> neighbors |> Dictionary.add (x, y) ((neighbors |> Dictionary.find (x, y)) + c))

    let newChanges =
        neighbors
        |> Dictionary.keys
        |> Seq.fold (fun acc (x, y) ->
            let current = bugs |> Dictionary.find (x, y)
            let neigh = neighbors |> Dictionary.find (x, y)
            match current with
            | Infested ->
                match neigh with
                | 1 -> acc
                | _ ->
                    bugs |> Dictionary.add (x, y) Empty
                    (x, y)
                    |> around
                    |> Seq.fold (fun acc (x, y) -> acc |> List.append [ (x, y, -1) ]) acc
            | Empty ->
                match neigh with
                | 1
                | 2 ->
                    bugs |> Dictionary.add (x, y) Infested
                    (x, y)
                    |> around
                    |> Seq.fold (fun acc (x, y) -> acc |> List.append [ (x, y, 1) ]) acc
                | _ -> acc) List.empty

    let bioScore = bugs |> score

    match history |> Set.contains bioScore with
    | true -> bioScore
    | false -> evolve bugs neighbors newChanges (history |> Set.add bioScore)