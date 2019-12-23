module Tests

open Xunit

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

  
let hasPair number =
  let (_, _, hasPair) =
    [ 5 .. -1 .. 0 ]
    |> Seq.fold (fun (n, previousDigit, hasPair) current ->
      match hasPair with
      | false ->
        let currentDigit = n / (pown 10 current)
        let nextNumber = n - currentDigit * (pown 10 current)

        match (currentDigit = previousDigit) with
        | true -> (nextNumber, currentDigit, true) 
        | false -> (nextNumber, currentDigit, false)
      | true -> (n, previousDigit, hasPair)) (number, 0, false)
        
  hasPair

let hasExactPair number =
  let (_, _, runLength, hasPair) =
    [ 5 .. -1 .. 0 ]
    |> Seq.fold (fun (n, previousDigit, runLength, hasPair) current ->
      let currentDigit = n / (pown 10 current)
      let nextNumber = n - currentDigit * (pown 10 current)

      match hasPair, currentDigit with
      | true, _             -> (nextNumber, currentDigit, runLength, hasPair)
      | false, pd when previousDigit = pd -> (nextNumber, currentDigit, runLength + 1, hasPair)
      | false, _            ->
        match runLength with
        | 2 -> (nextNumber, currentDigit, 1, true)
        | _ -> (nextNumber, currentDigit, 1, hasPair)
      ) (number, 0, 1, false)
        
  hasPair || runLength = 2

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