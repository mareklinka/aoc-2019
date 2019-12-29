module Dictionary

open System.Collections.Generic

let tryFind<'k, 'v> (k: 'k) (dict: Dictionary<'k, 'v>) =
    let (found, value) = dict.TryGetValue k
    match found with
    | true -> Some(value)
    | false -> None

let find<'k, 'v> (k: 'k) (dict: Dictionary<'k, 'v>) = dict.[k]

let findWithDefault<'k, 'v> (k: 'k) (d: 'v) (dict: Dictionary<'k, 'v>) =
    let (found, value) = dict.TryGetValue k
    match found with
    | true -> value
    | false -> d

let add<'k, 'v> (k: 'k) (v: 'v) (dict: Dictionary<'k, 'v>) =
    dict.[k] <- v
    ()

let toSeq<'k, 'v> (dict: Dictionary<'k, 'v>) = dict |> Seq.map (fun kvp -> (kvp.Key, kvp.Value))

let keys<'k, 'v> (dict: Dictionary<'k, 'v>) = dict.Keys |> seq

let values<'k, 'v> (dict: Dictionary<'k, 'v>) = dict.Values |> seq