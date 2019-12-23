module Tests

open Xunit
open System.IO

let rec CountOrbits body distance (map:Map<string, string array>) =
    let further =
        match map.TryFind body with
        | Some satellites ->
            satellites
            |> Array.map (fun s -> map |> CountOrbits s (distance + 1))
            |> Array.sum
        | None -> 0
        
    distance + further

let rec CountOrbitalTransfers target (map:Map<string, string array>) visited queue  =
    match queue with
    | [] -> failwith "Bruh, no path!"
    | (current, distance)::tail ->
        match visited |> Set.contains current with
        | true -> tail |> CountOrbitalTransfers target map visited // we already visited this node - skip to next
        | false -> 
            // haven't seen this node before
            let newVisited = visited |> Set.add current

            match current with
            // we end up here when we orbit SAN but the answer is the number of transfers to end up orbiting what SAN is orbiting
            // therefore - 1
            | current when current = target -> distance - 1  // final branch
            | current ->
                // centers might be 0 or 1 elements - COM orbits nothing
                let centers =
                    map
                    |> Map.filter (fun center _ -> Array.contains current map.[center]) // get KVPs where current orbits something
                    |> Map.toSeq
                    |> Seq.map fst // only project the keys - these are the orbital centers
                    |> Seq.toList  // so that I can pattern match          
    
                // get the satellites of the current node and combine match wigh current's orbital center
                match (map.TryFind current, centers) with
                | Some satellites, [center] -> 
                    // there are both satellites and a center - combine the two together and enqueue for processing
                    let newList =
                        satellites 
                        |> Seq.map (fun i -> (i, distance + 1)) 
                        |> List.ofSeq 
                        |> List.append [(center, distance+1)] 
                        |> List.append tail
                    newList |> CountOrbitalTransfers target map newVisited
                | Some satellites, _ ->
                    // only satellites - only enqueue those
                    let newList =
                        satellites 
                        |> Seq.map (fun i -> (i, distance + 1)) 
                        |> List.ofSeq
                        |> List.append tail
                    newList |> CountOrbitalTransfers target map newVisited
                | None, [center] -> 
                    // no satellites, just an orbital center 
                    [(center, distance + 1)] |> List.append tail |> CountOrbitalTransfers target map newVisited
                | None, _ ->
                    // nothing - this sould not happen 
                    tail |> CountOrbitalTransfers target map newVisited

[<Fact>]
let Part1() =
    let orbits =
        File.ReadAllLines("input.txt")
        |> Array.map (fun l -> (l.Split(")").[0], l.Split(")").[1]))
        |> Array.groupBy (fun (a,_) -> a)
        |> Array.map (fun (a, b) -> (a, b |> Array.map (fun (_,d) -> d)))
        |> Map.ofArray

    let orbitCount = orbits |> CountOrbits "COM" 0

    Assert.Equal(139597, orbitCount)

[<Fact>]
let Part2() =
    let orbits =
        File.ReadAllLines("input.txt")
        |> Array.map (fun line -> (line.Split(")").[0], line.Split(")").[1]))
        |> Array.groupBy (fun (center,_) -> center)
        |> Array.map (fun (center, pairs) -> (center, pairs |> Array.map (fun (_, satellite) -> satellite)))
        |> Map.ofArray

    let myCenter =
        orbits 
        |> Map.filter (fun center _ -> orbits.[center] |> Array.contains "YOU")
        |> Map.toSeq 
        |> Seq.map fst
        |> Seq.take 1
        |> List.ofSeq

    let visited = Set.empty |> Set.add "YOU"
    
    let orbitCount = CountOrbitalTransfers "SAN" orbits visited [(myCenter.[0], 0)] 

    Assert.Equal(286, orbitCount)