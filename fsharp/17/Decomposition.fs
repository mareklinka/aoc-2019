module Decomposition

let private verifyLength (part: Path.Instruction list) =
    let length = (part |> List.sumBy (fun i -> i.Length)) + (part |> List.length)
    length <= 15

let rec verify full part1 part2 part3 codes =
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

let rec private remove haystack needle acc =
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