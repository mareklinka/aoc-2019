module Tests

open Xunit
open System.IO
open System.Numerics

type Shuffle =
    | DealNew
    | Cut of int
    | Increment of int

let (|Prefix|_|) (p: string) (s: string) =
    if (s.StartsWith(p)) then Some(p) else None

let parseShuffles lines =
    lines
    |> Array.map (fun l ->
        match l with
        | Prefix "deal with increment" _ ->
            let param =
                l.Split(" ")
                |> Array.last
                |> int
            param |> Increment
        | Prefix "cut" _ ->
            let param =
                l.Split(" ")
                |> Array.last
                |> int
            param |> Cut
        | Prefix "deal into new" _ -> DealNew)

[<Fact>]
let Part1() =
    let shuffles = File.ReadAllLines("input.txt") |> parseShuffles
    let deckSize = 10007

    let a, b =
        shuffles
        |> Array.fold (fun (a, b) c ->
            match c with
            | DealNew -> (-a, -b - 1)
            | Cut n ->
                match n < 0 with
                | true -> (a, b - (n + deckSize))
                | false -> (a, b - n)
            | Increment n -> ((a * n) % deckSize, (b * n) % deckSize)) (1, 0)

    let result = (a * 2019 + b) % deckSize

    let result =
        match result < 0 with
        | true -> result + deckSize
        | false -> result

    Assert.Equal(4284, result)

[<Fact>]
let Part2() =
    let shuffles = File.ReadAllLines("input.txt") |> parseShuffles
    let deckSize = 119315717514047L |> bigint

    let sanitize i =
        match i < 0I with
        | true -> (i + deckSize) % deckSize
        | false -> i % deckSize

    let sanitizePair (x, y) = (x |> sanitize, y |> sanitize)

    let a, b =
        shuffles
        |> Array.rev
        |> Array.fold (fun (a, b) c ->
            match c with
            | DealNew -> (-a, -b - 1I)
            | Cut n ->
                match n < 0 with
                | true -> (a, b + ((n |> bigint) + deckSize))
                | false -> (a, b + (n |> bigint))
            | Increment n ->
                let inverseMod = BigInteger.ModPow(n |> bigint, deckSize - 2I, deckSize)
                ((a * inverseMod) % deckSize, (b * inverseMod) % deckSize)
            |> sanitizePair) (1I, 0I)

    let shuffleCount = 101741582076661I
    let part1 = 2020I * BigInteger.ModPow(a, shuffleCount, deckSize)
    let part2 = b * (BigInteger.ModPow(a, shuffleCount, deckSize) - 1I)
    let part3 = BigInteger.ModPow(a - 1I, deckSize - 2I, deckSize)

    let result = (part1 + part2 * part3) % deckSize

    Assert.Equal(96797432275571I, result)