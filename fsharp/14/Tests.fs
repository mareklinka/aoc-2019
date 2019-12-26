module Tests

open System
open Xunit
open System.IO

type Chemical = Code of string

type Component =
    { Chemical: Chemical
      Quantity: int64 }

type Reaction =
    { Target: Component
      Sources: Component list }

let rec private reactorRec reactions { Chemical = target; Quantity = demand } tank =
    match target with
    | Code "ORE" -> (demand, tank)
    | _ ->
        let reaction = reactions |> List.find (fun r -> r.Target.Chemical = target)
        let inTank = tank |> Map.tryFind target // we might have something in the tank already

        let remainder =
            match inTank with
            | Some(remainder) -> remainder
            | None -> 0L

        match remainder >= demand with
        | true -> (0L, tank |> Map.add target (remainder - demand)) // remainder is enough to satisfy demand
        | false ->
            (*
                remainder was not enough to satisfy demand
                we need to create AT LEAST as much of the desired chemical so that remainder + created >= demand
            *)

            let needToCreate = demand - remainder // this much we need to create AT LEAST
            let numberOfReactions =
                Math.Ceiling((needToCreate |> float) / (reaction.Target.Quantity |> float))
                |> int64 // this many reactions will be required

            // go over the reaction's sources and try and get them from the tank recursively
            // the accumulator here is a tuple of (ore required to get the source component, new state of the chemical tank)
            let (totalReactionOre, tankAfterReaction) =
                reaction.Sources
                |> List.fold (fun (oreAcc, tankAcc) sourceComponent ->
                    let (componentOre, componentTank) =
                        reactorRec reactions
                            { sourceComponent with Quantity = sourceComponent.Quantity * numberOfReactions } tankAcc
                    (oreAcc + componentOre, componentTank)) (0L, tank)

            (*
                there might be some chemicals left over - some reactions create N of the target but we might require less
                this difference gets put into tank as a remainder so taht next time we hit the same chemical,
                we don't need to create everything from scratch
            *)
            let created = reaction.Target.Quantity * numberOfReactions
            let toPutIntoTank = remainder + created - demand
            (totalReactionOre, tankAfterReaction |> Map.add reaction.Target.Chemical toPutIntoTank)


let reactor reactions { Chemical = c; Quantity = q } =
    reactorRec reactions
        { Chemical = c
          Quantity = q } Map.empty
    |> fst

let rec private binaryFuelSearch reactions oreAvailable lowerBound upperBound =
    match upperBound - lowerBound with
    | 0L
    | 1L -> lowerBound
    | _ ->
        let half = (upperBound + lowerBound) / 2L

        let oreConsumed =
            reactor reactions
                { Chemical = "FUEL" |> Code
                  Quantity = half }

        match oreConsumed < oreAvailable with
        | true -> binaryFuelSearch reactions oreAvailable half upperBound
        | false -> binaryFuelSearch reactions oreAvailable lowerBound half

let private parseReactions path =
    File.ReadAllLines(path)
    |> Array.map (fun l ->
        let split = l.Split(" => ")

        let sources =
            split.[0].Split(", ")
            |> Array.map (fun s ->
                let split = s.Split(" ")
                { Component.Chemical = split.[1] |> Code
                  Component.Quantity = split.[0] |> int64 })
            |> List.ofArray

        let targetSplit = split.[1].Split(" ")

        let target =
            { Component.Chemical = targetSplit.[1] |> Code
              Component.Quantity = targetSplit.[0] |> int64 }
        { Reaction.Target = target
          Reaction.Sources = sources })
    |> List.ofArray

[<Fact>]
let Part1() =
    let reactions = parseReactions "input.txt"

    let ore =
        reactor reactions
            { Component.Chemical = "FUEL" |> Code
              Component.Quantity = 1L }

    Assert.Equal(873899L, ore)

[<Fact>]
let Part2() =
    let reactions = parseReactions "input.txt"
    let harvestedOre = 1000000000000L
    let lowerBound = harvestedOre / 873899L
    let upperBound = lowerBound * 2L

    let x = binaryFuelSearch reactions harvestedOre lowerBound upperBound

    Assert.Equal(1893569L, x)
