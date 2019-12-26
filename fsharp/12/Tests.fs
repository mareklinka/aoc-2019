module Tests

open Xunit
open System.IO
open System

type Axis =
    | X
    | Y
    | Z

type Moon =
    { X: int
      Y: int
      Z: int
      VX: int
      VY: int
      VZ: int }
    member __.Energy =
        (Math.Abs(__.X) + Math.Abs(__.Y) + Math.Abs(__.Z)) * (Math.Abs(__.VX) + Math.Abs(__.VY) + Math.Abs(__.VZ))

    member __.Dim axis =
        match axis with
        | X -> (__.X, __.VX)
        | Y -> (__.Y, __.VY)
        | Z -> (__.Z, __.VZ)

let timeStep (moons: Moon list) =
    moons
    |> List.fold (fun a m ->
        let (vx, vy, vz) =
            moons
            |> List.filter (fun other -> m <> other)
            |> List.fold (fun (dx, dy, dz) other ->
                let newDx = other.X.CompareTo(m.X)
                let newDy = other.Y.CompareTo(m.Y)
                let newDz = other.Z.CompareTo(m.Z)

                (dx + newDx, dy + newDy, dz + newDz)) (m.VX, m.VY, m.VZ)
        [ { X = m.X + vx
            Y = m.Y + vy
            Z = m.Z + vz
            VX = vx
            VY = vy
            VZ = vz } ]
        |> List.append a) []

let rec findPeriod mapFunction cache moons =
    let key = moons |> mapFunction
    match cache |> Set.contains key with
    | true -> cache |> Set.count
    | false ->
        let newMoons = moons |> timeStep
        findPeriod mapFunction (cache |> Set.add key) newMoons

let parser =
    (fun (l: string) -> l.Split(", "))
    >> (fun s -> (s.[0].[3..] |> int, s.[1].[2..] |> int, s.[2].[2..(s.[2].Length - 2)] |> int))
    >> (fun (x, y, z) ->
        { Moon.X = x
          Moon.Y = y
          Moon.Z = z
          Moon.VX = 0
          Moon.VY = 0
          Moon.VZ = 0 })

let rec gcd x y =
    if y = 0L then abs x else gcd y (x % y)

let lcm x y = x * y / (gcd x y)

[<Fact>]
let Part1() =
    let initialState =
        File.ReadAllLines("input.txt")
        |> Array.map parser
        |> List.ofArray

    let after = { 1 .. 1000 } |> Seq.fold (fun a _ -> a |> timeStep) initialState

    let energy = after |> List.sumBy (fun m -> m.Energy)

    Assert.Equal(6678, energy)

[<Fact>]
let Part2() =
    let initialState =
        File.ReadAllLines("input.txt")
        |> Array.map parser
        |> List.ofArray

    let projector axis (ms: Moon list) = (ms.[0].Dim axis, ms.[1].Dim axis, ms.[2].Dim axis, ms.[3].Dim axis)

    // this could be simplified to become a single-pass search but this is more descriptive
    let xPeriod =
        initialState
        |> findPeriod (projector X) Set.empty
        |> int64

    let yPeriod =
        initialState
        |> findPeriod (projector Y) Set.empty
        |> int64

    let zPeriod =
        initialState
        |> findPeriod (projector Z) Set.empty
        |> int64

    let totalPeriod = (xPeriod |> lcm yPeriod) |> lcm zPeriod

    Assert.Equal(496734501382552L, totalPeriod)
