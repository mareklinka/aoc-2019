module Tests

open Xunit


/// **Description**
/// Checks if the provided number is composed of non-decreasing digits (left to right)
///
/// **Parameters**
///   * `number` - the number to check
///
/// **Output Type**
///   * `bool` - `true` if the number has non-decrfeasing digits, otherwise false
let isIncreasing number =
    let (_, _, nonDecreasing) =
        [ 5 .. -1 .. 0 ]
        |> Seq.fold (fun (n, previousDigit, nonDecreasing) current ->
            match nonDecreasing with
            | false -> (n, previousDigit, false)
            | true ->
                let currentDigit = n / (pown 10 current)
                let nextNumber = n - currentDigit * (pown 10 current)

                match (currentDigit < previousDigit) with
                | true -> (nextNumber, currentDigit, false)
                | false -> (nextNumber, currentDigit, true)) (number, 0, true)

    nonDecreasing



/// **Description**
/// Checks if the provided number contains any digit twice in a row. E.g. 1123456 or 123455
///
/// **Parameters**
///   * `number` - the number to check
///
/// **Output Type**
///   * `bool` - `true` if the number contains any digit twice in a row, otherwise false
let hasPair number =
    let (_, _, hasPair) =
        [ 5 .. -1 .. 0 ]
        |> Seq.fold (fun (n, previousDigit, hasPair) current ->
            match hasPair with
            | true -> (n, previousDigit, hasPair) // short circuit
            | false ->
                let currentDigit = n / (pown 10 current)
                let nextNumber = n - currentDigit * (pown 10 current)

                match (currentDigit = previousDigit) with
                | true -> (nextNumber, currentDigit, true)
                | false -> (nextNumber, currentDigit, false)) (number, 0, false)

    hasPair


/// **Description**
/// Checks if the provided number contains any digit exactly twice in a row
///
/// **Parameters**
///   * `number` - the number to check
///
/// **Output Type**
///   * `bool` - `true` if the number contains any digit exactly twice in a row, otherwise false
let hasExactPair number =
    let (_, _, runLength, hasPair) =
        [ 5 .. -1 .. 0 ]
        |> Seq.fold (fun (n, previousDigit, runLength, hasPair) current ->
            let currentDigit = n / (pown 10 current)
            let nextNumber = n - currentDigit * (pown 10 current)

            match hasPair, currentDigit with
            | true, _ -> (nextNumber, currentDigit, runLength, hasPair) // short circuit
            | _, pd when previousDigit = pd -> (nextNumber, currentDigit, runLength + 1, hasPair) // lengthen run
            | _, _ ->
                match runLength with
                | 2 -> (nextNumber, currentDigit, 1, true)
                | _ -> (nextNumber, currentDigit, 1, hasPair)) (number, 0, 1, false)

    hasPair || runLength = 2 // the 2-run might be the last pair of digits in the number

[<Theory>]
[<InlineData(123456, true)>]
[<InlineData(111111, true)>]
[<InlineData(111999, true)>]
[<InlineData(119990, false)>]
[<InlineData(987123, false)>]
let IsIncreasingTests number expected = Assert.Equal(expected, number |> isIncreasing)

[<Theory>]
[<InlineData(112233, true)>]
[<InlineData(111111, true)>]
[<InlineData(123455, true)>]
[<InlineData(123456, false)>]
[<InlineData(737583, false)>]
let HasPairTests number expected = Assert.Equal(expected, number |> hasPair)

[<Theory>]
[<InlineData(112233, true)>]
[<InlineData(111111, false)>]
[<InlineData(123455, true)>]
[<InlineData(123456, false)>]
[<InlineData(737583, false)>]
let HasExactPairTests number expected = Assert.Equal(expected, number |> hasExactPair)

[<Fact>]
let Part1() =
    let range = [ 272091 .. 815432 ]

    let l =
        range
        |> Seq.filter (fun n -> isIncreasing n && hasPair n)
        |> Seq.length

    Assert.Equal(931, l)

[<Fact>]
let Part2() =
    let range = [ 272091 .. 815432 ]

    let l =
        range
        |> Seq.filter (fun n -> isIncreasing n && hasExactPair n)
        |> Seq.length

    Assert.Equal(609, l)
 