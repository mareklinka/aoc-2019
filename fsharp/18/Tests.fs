module Tests

open System
open Xunit
open System.IO
open System.Collections.Generic

type Keychain = Keys of int

type Key = Key of int

let addKey keychain key =
    let (Keys kc) = keychain
    let (Key k) = key
    (kc ||| k) |> Keys

let hasKey keychain key =
    let (Keys kc) = keychain
    let (Key k) = key
    (kc &&& k) = k

type Keychain with
    member this.AddKey k = addKey this k
    member this.HasKey k = hasKey this k

type Position =
    {
        P1 : int * int
        P2 : int * int
        P3 : int * int
        P4 : int * int
    }

    member __.Item i =
        match i with
        | 0 -> __.P1
        | 1 -> __.P2
        | 2 -> __.P3
        | 3 -> __.P4
    member __.Set i v =
        match i with
        | 0 ->
            {
                __ with
                    P1 = v
            }
        | 1 ->
            {
                __ with
                    P2 = v
            }
        | 2 ->
            {
                __ with
                    P3 = v
            }
        | 3 ->
            {
                __ with
                    P4 = v
            }

let around (x, y) =
    seq {
        (x - 1, y)
        (x + 1, y)
        (x, y - 1)
        (x, y + 1)
    }

let rec reachableKeys map visited acc q =
    let helper (x, y) dist door =
        (x, y)
        |> around
        |> Seq.filter (fun p -> not (visited |> Set.contains p))
        |> Seq.map (fun p -> (p, dist, door))
        |> List.ofSeq

    match q with
    | [] -> acc
    | ((x, y), distance, doors) :: tail ->
        match visited |> Set.contains (x, y) with
        | true -> reachableKeys map visited acc tail
        | false ->
            let newVisited = visited |> Set.add (x, y)
            let current = map |> Map.find (x, y)
            match current with
            | '#' -> reachableKeys map newVisited acc tail
            | c when c |> Char.IsUpper ->
                let requiredKey =
                    (pown 2 ((c
                      |> Char.ToLower
                      |> int)
                     - ('a' |> int)))
                    |> Key

                let newDoors = requiredKey |> addKey doors

                let toEnqueue = helper (x, y) (distance + 1) newDoors
                reachableKeys map newVisited acc (tail @ toEnqueue)
            | c when c |> Char.IsLower ->
                let newAcc = acc |> List.append [ x, y, distance, doors ]
                let toEnqueue = helper (x, y) (distance + 1) doors
                reachableKeys map newVisited newAcc (tail @ toEnqueue)
            | '.'
            | '@'
            | '1'
            | '2'
            | '3'
            | '4' ->
                let toEnqueue = helper (x, y) (distance + 1) doors
                reachableKeys map newVisited acc (tail @ toEnqueue)

module Queue =
    let dequeue<'a> (q: Queue<'a>) = q.Dequeue()

    let count<'a> (q: Queue<'a>) = q.Count

    let enqueue<'a> (v:'a) (q: Queue<'a>) = q.Enqueue(v)

    let enqueueAll<'a> (v:seq<'a>) (q: Queue<'a>) =
        v |> Seq.iter q.Enqueue

module Dictionary =
    let tryFind<'k, 'v> (k: 'k) (dict: Dictionary<'k, 'v>) =
        let (found, value) = dict.TryGetValue k
        match found with
        | true -> Some(value)
        | false -> None

    let find<'k, 'v> (k: 'k) (dict: Dictionary<'k, 'v>) =
        dict.[k]

    let add<'k, 'v> (k: 'k) (v: 'v) (dict: Dictionary<'k, 'v>) =
        dict.[k] <- v
        ()

let rec collectKeys map paths allKeys robotCount visited acc
    (q: Queue<Position * int * Keychain>) =
    // the performance of this is rather abyssmal but that's the price of being immutable and functional
    // introducing mutable dictionaries was necessary to get the time down to something reasonable
    match q |> Queue.count with
    | 0 -> acc
    | _ ->
        let (pos, distance, keys) = q |> Queue.dequeue
        let hasVisited = visited |> Dictionary.tryFind (pos, keys)
        match hasVisited with
        | Some(previousDistance) when previousDistance <= distance -> collectKeys map paths allKeys robotCount visited acc q
        | _ ->
            visited |> Dictionary.add (pos, keys) distance
            match keys with
            | k when k = allKeys ->
                // done - we collected all the keys
                let newAcc = min acc distance
                collectKeys map paths allKeys robotCount visited newAcc q
            | _ ->
                { 0 .. robotCount - 1 }
                |> Seq.iter (fun robot ->
                    let (x,y) = pos.Item robot
                    let openPaths =
                        paths
                        |> Map.find (x, y)
                        |> Seq.filter (fun (x, y, _, (Keys k)) ->
                            let isPathOpen = keys.HasKey(k |> Key)
                            match isPathOpen with
                            | false -> false
                            | true ->
                                let keyAtTarget = map |> Dictionary.find (x, y)
                                let alreadyHasKey = hasKey keys keyAtTarget
                                not alreadyHasKey)
                        |> Seq.map (fun (x, y, d, _) ->
                            (pos.Set robot (x,y), distance + d, (addKey keys (map |> Dictionary.find (x, y)))))
                    q |> Queue.enqueueAll openPaths
                )

                collectKeys map paths allKeys robotCount visited acc q

[<Fact>]
let Part1() =
    let input = File.ReadAllLines("input.txt")
    let lineLength = input.[0].Length

    let (map, _, _) =
        String.Join("", input).ToCharArray()
        |> Array.fold (fun (m, x, y) c ->
            let newMap = m |> Map.add (x, y) c
            let newX = (x + 1) % lineLength
            match newX with
            | 0 -> (newMap, newX, y + 1)
            | _ -> (newMap, newX, y)) (Map.empty, 0, 0)

    let keyPositions =
        map
        |> Map.filter (fun _ v -> v |> Char.IsLower)
        |> Map.toSeq
        |> Seq.map (fun (k, _) -> k)
        |> List.ofSeq

    let startPositions =
        map
        |> Map.filter (fun _ v -> v = '@')
        |> Map.toSeq
        |> Seq.map (fun (k, _) -> k)
        |> List.ofSeq

    let paths =
        keyPositions
        |> List.append startPositions
        |> List.fold (fun acc c ->
            let p = reachableKeys map Set.empty List.empty [ (c, 0, 0 |> Keys) ]
            acc |> Map.add c p) Map.empty

    let keyCount = keyPositions |> List.length

    let targetKeychain =
        { 0 .. keyCount - 1 }
        |> Seq.fold (fun acc i -> acc ||| (pown 2 i)) 0
        |> Keys

    let q = Queue<Position * int * Keychain>()
    let startState =
        {
            P1 = startPositions.[0]
            P2 = (0,0)
            P3 = (0,0)
            P4 = (0,0)
        }
    q.Enqueue((startState, 0, 0 |> Keys))

    let keyDict = Dictionary<int*int, Key>()
    map
        |> Map.filter (fun _ v -> Char.IsLower v)
        |> Map.map (fun _ v -> (pown 2 ((v |> int) - ('a' |> int))) |> Key)
        |> Map.iter (fun k v -> keyDict.Add(k, v))
    let x = collectKeys keyDict paths targetKeychain 1 (Dictionary<Position * Keychain, int>()) Int32.MaxValue q

    Assert.Equal(4520, x)

[<Fact>]
let Part2() =
    let input = File.ReadAllLines("input2.txt")
    let lineLength = input.[0].Length

    let (map, _, _) =
        String.Join("", input).ToCharArray()
        |> Array.fold (fun (m, x, y) c ->
            let newMap = m |> Map.add (x, y) c
            let newX = (x + 1) % lineLength
            match newX with
            | 0 -> (newMap, newX, y + 1)
            | _ -> (newMap, newX, y)) (Map.empty, 0, 0)

    let keyPositions =
        map
        |> Map.filter (fun _ v -> v |> Char.IsLower)
        |> Map.toSeq
        |> Seq.map (fun (k, _) -> k)
        |> List.ofSeq

    let startPositions =
        map
        |> Map.filter (fun _ v -> v = '1' || v = '2' || v = '3' || v = '4')
        |> Map.toSeq
        |> Seq.map (fun (k, _) -> k)
        |> List.ofSeq

    let paths =
        keyPositions
        |> List.append startPositions
        |> List.fold (fun acc c ->
            let p = reachableKeys map Set.empty List.empty [ (c, 0, 0 |> Keys) ]
            acc |> Map.add c p) Map.empty

    let keyCount = keyPositions |> List.length

    let targetKeychain =
        { 0 .. keyCount - 1 }
        |> Seq.fold (fun acc i -> acc ||| (pown 2 i)) 0
        |> Keys

    let q = Queue<Position * int * Keychain>()
    let startState =
        {
            P1 = startPositions.[0]
            P2 = startPositions.[1]
            P3 = startPositions.[2]
            P4 = startPositions.[3]
        }
    q.Enqueue((startState, 0, 0 |> Keys))

    let keyDict = Dictionary<int*int, Key>()
    map
        |> Map.filter (fun _ v -> Char.IsLower v)
        |> Map.map (fun _ v -> (pown 2 ((v |> int) - ('a' |> int))) |> Key)
        |> Map.iter (fun k v -> keyDict.Add(k, v))
    let x = collectKeys keyDict paths targetKeychain 4 (Dictionary<Position * Keychain, int>()) Int32.MaxValue q

    Assert.Equal(1540, x)
