module Tests

open System
open Xunit
open System.IO
open System.Collections.Generic

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

module HashSet =
    let contains<'a> (v:'a) (set:HashSet<'a>) =
        set.Contains v

    let add<'a> (v:'a) (set:HashSet<'a>) =
        set.Add v |> ignore
        ()

let around (x, y) =
    seq {
        (x - 1, y)
        (x + 1, y)
        (x, y - 1)
        (x, y + 1)
    }

let rec walkNormalMaze map portals endPoint visited q =
    match q with
    | (x,y, distance)::_ when (x,y) = endPoint ->
        distance
    | (x,y, distance)::tail ->
        visited |> HashSet.add (x,y)
        let toEnqueue =
            (x,y)
            |> around
            |> Seq.filter (fun (x,y) ->
                match map |> Map.tryFind (x,y) with
                | Some('.') -> true
                | _ -> false)
            |> Seq.filter (fun (x,y) -> not (visited |> HashSet.contains (x,y)))
            |> Seq.map (fun (x,y) -> (x,y, distance + 1))
            |> Seq.append (
                match portals |> Map.tryFind (x,y) with
                | Some(a,b) ->
                    seq { (a,b, distance + 1) }
                | None ->
                    Seq.empty)
            |> List.ofSeq

        walkNormalMaze map portals endPoint visited (tail @ toEnqueue)

let rec walkRecursiveMaze map portals endPoint (minX, maxX, minY, maxY) visited q =
    let state = q |> Queue.dequeue
    match state with
    | (x,y, distance, 0) when (x,y) = endPoint ->
        distance
    | (x,y, distance, level) ->
        visited |> HashSet.add (x,y,level)
        let toEnqueue =
            (x,y)
            |> around
            |> Seq.filter (fun (x,y) ->
                match map |> Dictionary.tryFind (x,y) with
                | Some('.') -> true
                | _ -> false)
            |> Seq.map (fun (x,y) -> (x,y, distance + 1, level))
            |> Seq.append (
                match portals |> Dictionary.tryFind (x,y) with
                | Some(a,b) ->
                    let isOuterPortal = (x = minX) || (x = maxX) || (y = minY) || (y = maxY)
                    match isOuterPortal with
                    | true ->
                        match level with
                        | 0 ->
                            Seq.empty
                        | _ ->
                            seq { (a,b, distance + 1, level - 1) }
                    | false ->
                          seq { (a,b, distance + 1, level + 1) }
                | None ->
                    Seq.empty)
            |> Seq.filter (fun (x,y,_,l) -> not (visited |> HashSet.contains (x,y, l)))
            |> List.ofSeq

        q |> Queue.enqueueAll toEnqueue

        walkRecursiveMaze map portals endPoint (minX, maxX, minY, maxY) visited q

let getPortalName (x1,y1) (x2,y2) map =
    ((map |> Map.find (x1, y1)) |> string) + ((map |> Map.find (x2, y2)) |> string)

let parseMaze filename =
    let lines = File.ReadAllLines filename
    let lineLength = lines.[0].Length

    let (map, _, _) =
        String.Join(String.Empty, lines).ToCharArray()
        |> Array.fold (fun (map, x, y) c ->
            let newMap = map |> Map.add (x, y) c
            let newX = (x + 1) % lineLength

            let newY =
                match newX with
                | 0 -> y + 1
                | _ -> y
            (newMap, newX, newY)) (Map.empty, 0, 0)

    let portalPoints =
        map
        |> Map.toSeq
        |> Seq.filter (fun ((x, y), v) ->
            v = '.' && ((x, y)
                        |> around
                        |> Seq.exists (fun (x, y) ->
                            map
                            |> Map.find (x, y)
                            |> Char.IsUpper)))
        |> Seq.map (fun ((x, y), _) ->
            let namePartPos =
                (x, y)
                |> around
                |> Seq.filter (fun (a, b) ->
                    map
                    |> Map.find (a, b)
                    |> Char.IsUpper)
                |> Seq.exactlyOne
            match namePartPos with
            | (a, b) when (a, b) = (x - 1, y) ->
                let name = map |> getPortalName (x - 2, y) (x - 1, y)
                ((x, y), name)
            | (a, b) when (a, b) = (x + 1, y) ->
                let name = map |> getPortalName (x + 1, y) (x + 2, y)
                ((x, y), name)
            | (a, b) when (a, b) = (x, y - 1) ->
                let name = map |> getPortalName (x, y - 2) (x, y - 1)
                ((x, y), name)
            | (a, b) when (a, b) = (x, y + 1) ->
                let name = map |> getPortalName (x, y + 1) (x, y + 2)
                ((x, y), name)
        ) |> Seq.toList

    let ((startX, startY), _) =
        portalPoints
        |> Seq.filter (fun (_, name) -> name = "AA")
        |> Seq.exactlyOne

    let ((endX, endY), _) =
        portalPoints
        |> Seq.filter (fun (_, name) -> name = "ZZ")
        |> Seq.exactlyOne

    let portals =
        portalPoints
        |> Seq.groupBy (fun (_, name) -> name)
        |> Seq.filter (fun (_, group) -> group |> Seq.length = 2)
        |> Seq.fold (fun acc (_, group) ->
            let (x1, y1), _ = group |> Seq.item 0
            let (x2, y2), _ = group |> Seq.item 1
            let newAcc = acc |> Map.add (x1, y1) (x2, y2) |> Map.add (x2, y2) (x1, y1)
            newAcc) (Map.empty)

    map,portals,(startX, startY),(endX,endY)

[<Fact>]
let Part1() =
    let (map, portals, (startX, startY), endPos) = parseMaze "input.txt"

    let result = walkNormalMaze map portals endPos (HashSet()) [(startX, startY, 0)]

    Assert.Equal(570, result)

[<Fact>]
let Part2() =
    let (map, portals, (startX, startY), endPos) = parseMaze "input.txt"

    let boundaries =
        portals
        |> Map.fold (fun (a,b,c,d) (m, n) (_, _) ->
            (min a m, max b m, min c n, max d n)
        ) (Int32.MaxValue, Int32.MinValue, Int32.MaxValue, Int32.MinValue)

    // moving to mutable data structures with better access complexity (O(1))
    // Map and Set are tree-based, so O(log N)
    let mapDict = Dictionary<int * int, char>()
    map |> Map.iter (fun k v -> mapDict.Add(k,v))

    let portalDict = Dictionary<int * int, int * int>()
    portals |> Map.iter (fun k v -> portalDict.Add(k,v))

    let q = Queue<int * int * int * int>()
    q |> Queue.enqueue (startX, startY, 0, 0)

    let result = walkRecursiveMaze mapDict portalDict endPos boundaries (HashSet()) q

    Assert.Equal(7056, result)