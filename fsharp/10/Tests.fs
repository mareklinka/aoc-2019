module Tests

open System
open Xunit
open System.IO

let parseAsteroidField path =
    let lines = File.ReadAllLines(path)
    let lineWidth = lines.[0].Length
    let concat = String.Join("", lines).ToCharArray()

    let (_, _, asteroidField) =
        concat
        |> Array.fold (fun (row, col, set) c ->
            let newSet =
                match c with
                | '#' -> set |> Set.add (col, row)
                | _ -> set

            let nextCol = (col + 1) % lineWidth

            let nextRow =
                match nextCol with
                | 0 -> row + 1
                | _ -> row
            (nextRow, nextCol, newSet)) (0, 0, Set.empty)
    asteroidField

let findBestPostion field =
    field
    |> Set.fold (fun (bestX, bestY, bestVisible) (x, y) ->
        let visible =
            field
            |> Set.fold (fun angles (xx, yy) ->
                match (xx, yy) with
                | (xx, yy) when (xx = x) && (yy = y) -> angles
                | _ ->
                    let angle = Math.Atan2((yy - y) |> float, (xx - x) |> float) * 180.0 / Math.PI
                    Set.add angle angles) Set.empty
            |> Set.count

        match visible > bestVisible with
        | true -> (x, y, visible)
        | false -> (bestX, bestY, bestVisible)) (-1, -1, 0)


[<Fact>]
let Part1() =
    let asteroidField = parseAsteroidField "input.txt"
    let (_, _, best) = asteroidField |> findBestPostion

    Assert.Equal(227, best)

[<Fact>]
let Part2() =
    let asteroidField = parseAsteroidField "input.txt"
    let (centerX, centerY, _) = asteroidField |> findBestPostion

    // looking for a 200th asteroid
    // since I can see 227 (part 1), I don't do a full revoluion of the laser
    // therefore I just need the closest asteroid that is under the 200th angle
    let allAngles =
        asteroidField
        |> Set.fold (fun a (x, y) ->
            match (x, y) with
            | (x, y) when (x = centerX) && (y = centerY) -> a
            | _ ->
                let angle = Math.Atan2((y - centerY) |> float, (x - centerX) |> float) * 180.0 / Math.PI

                let monotonicAngle =
                    match angle < 0.0 with
                    | true -> 360.0 + angle
                    | false -> angle
                a |> List.append [ (monotonicAngle, x, y) ]) List.empty

    let angleGroups = allAngles |> List.groupBy (fun (a, _, _) -> a)

    let uniqueAngles =
        angleGroups
        |> List.map fst
        |> List.sortBy id

    let startIndex = uniqueAngles |> List.findIndex (fun a -> a = 270.0)
    let targetIndex = (startIndex + 199) % (uniqueAngles |> List.length)
    let targetAngle = uniqueAngles.[targetIndex]

    let (_, asteroidsUnderAngle) = angleGroups |> List.find (fun (a, _) -> a = targetAngle)

    let (x, y, _) =
        asteroidsUnderAngle
        |> List.fold (fun (ax, ay, ad) (_, x, y) ->
            let distance = Math.Abs(ax - x) + Math.Abs(ay - y)
            match distance < ad with
            | true -> (x, y, distance)
            | false -> (ax, ay, ad)) (-1, -1, 1000)

    Assert.Equal(604, x * 100 + y)
