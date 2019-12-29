[<RequireQualifiedAccess>]
module Part2

let around rows cols (x, y, l) =
    seq {
        match y - 1 < 0 with
        | true -> yield (2, 1, l - 1)
        | false ->
            match (x, y) with
            | (2, 3) -> yield! ({ 0 .. 4 } |> Seq.map (fun x -> (x, rows - 1, l + 1)))
            | _ -> yield (x, y - 1, l)

        match y + 1 >= rows with
        | true -> yield (2, 3, l - 1)
        | false ->
            match (x, y) with
            | (2, 1) -> yield! ({ 0 .. 4 }|> Seq.map (fun x -> (x, 0, l + 1)))
            | _ -> yield (x, y + 1, l)

        match x - 1 < 0 with
        | true -> yield (1, 2, l - 1)
        | false ->
            match (x, y) with
            | (3, 2) -> yield! ({ 0 .. 4 }|> Seq.map (fun y -> (cols - 1, y, l + 1)))
            | _ -> yield (x - 1, y, l)

        match x + 1 >= cols with
        | true -> yield (3, 2, l - 1)
        | false ->
            match (x, y) with
            | (1, 2) -> yield! ({ 0 .. 4 }|> Seq.map (fun y -> (0, y, l + 1)))
            | _ -> yield (x + 1, y, l)
    }

let aroundInfested map (x, y, l) =
    (x, y, l)
    |> around 5 5
    |> Seq.sumBy (fun (x, y, l) ->
        match map |> Dictionary.tryFind (x, y, l) with
        | Some(Infested) -> 1
        | _ -> 0)

let rec evolve bugs neighbors =
    { 1 .. 200 }
    |> Seq.fold (fun changes _ ->
        changes
        |> List.iter (fun (x, y, l, c) -> neighbors |> Dictionary.add (x, y, l) ((neighbors |> Dictionary.findWithDefault (x, y, l) 0) + c))

        let newChanges =
            neighbors
            |> Dictionary.keys
            |> Seq.fold (fun acc (x, y, l) ->
                let current = bugs |> Dictionary.findWithDefault (x, y, l) Empty
                let neigh = neighbors |> Dictionary.findWithDefault (x, y, l) 0
                match current with
                | Infested ->
                    match neigh with
                    | 1 -> acc
                    | _ ->
                        bugs |> Dictionary.add (x, y, l) Empty
                        (x, y, l)
                        |> around 5 5
                        |> Seq.fold (fun acc (x, y, l) -> acc |> List.append [ (x, y, l, -1) ]) acc
                | Empty ->
                    match neigh with
                    | 1
                    | 2 ->
                        bugs |> Dictionary.add (x, y, l) Infested
                        (x, y, l)
                        |> around 5 5
                        |> Seq.fold (fun acc (x, y, l) -> acc |> List.append [ (x, y, l, 1) ]) acc
                    | _ -> acc) List.empty
        newChanges) []
        |> ignore

    bugs |> Dictionary.values |> Seq.filter (fun t -> t = Infested) |> Seq.length