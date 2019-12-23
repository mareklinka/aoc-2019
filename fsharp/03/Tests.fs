module Tests

open Xunit

let ParseWire(parts: string list) =
  let _, _, _, map =
    parts
    |> List.fold (fun (x, y, d, m) current ->
      let dir = current.[0]
      let count = current.[1..] |> int

      [ 1 .. count ]
      |> Seq.fold (fun (x, y, d, m) _ ->
        let nextX, nextY =
          match dir with
          | 'R' -> (x + 1, y)
          | 'L' -> (x - 1, y)
          | 'U' -> (x, y - 1)
          | 'D' -> (x, y + 1)

        let withPoint = Map.add (nextX, nextY) (d + 1) m
        nextX, nextY, d + 1, withPoint) (x, y, d, m)) (0, 0, 0, Map.empty)
  map

let FindClosestIntersection (wire1:Map<int * int, int>) wire2 =
  let keys1 = wire1 |> Map.toSeq |> Seq.map fst |> set
  let keys2 = wire2 |> Map.toSeq |> Seq.map fst |> set

  let intersections = Set.intersect keys1 keys2
  
  let minDistance =
    intersections
    |> Set.fold (fun min (x,y) ->
      let currentDistance = System.Math.Abs(x) + System.Math.Abs(y)
      match currentDistance < min with
      | true  -> currentDistance
      | false -> min
      ) (2147483647)

  minDistance

let FindFastestIntersection (wire1:Map<int * int, int>) wire2 =
  let keys1 = wire1 |> Map.toSeq |> Seq.map fst |> set
  let keys2 = wire2 |> Map.toSeq |> Seq.map fst |> set

  let intersections = Set.intersect keys1 keys2
  
  let minSteps =
    intersections
    |> Set.fold (fun min (x,y) ->
      let currentSteps = wire1.[(x,y)] + wire2.[(x,y)]
      match currentSteps < min with
      | true  -> currentSteps
      | false -> min
      ) (2147483647)

  minSteps  

[<Theory>]
[<InlineData("R8,U5,L5,D3", "U7,R6,D4,L4", 6)>]
[<InlineData("R75,D30,R83,U83,L12,D49,R71,U7,L72", "U62,R66,U55,R34,D71,R55,D58,R83", 159)>]
[<InlineData("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51", "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7", 135)>]
let Part1SmallTests(wire1String: string, wire2String: string, expected) =
  let wire1 =
    wire1String.Split ","
    |> Array.toList
    |> ParseWire

  let wire2 =
    wire2String.Split ","
    |> Array.toList
    |> ParseWire

  let minDistance = FindClosestIntersection wire1 wire2

  Assert.Equal(expected, minDistance)

[<Fact>]
let Part1() =
  let lines = System.IO.File.ReadAllLines("input.txt")
  let wire1 = lines.[0].Split(",")|> Array.toList |> ParseWire
  let wire2 = lines.[1].Split(",")|> Array.toList |> ParseWire

  let minDistance = FindClosestIntersection wire1 wire2

  Assert.Equal(260, minDistance)

[<Fact>]
let Part2() =
  let lines = System.IO.File.ReadAllLines("input.txt")
  let wire1 = lines.[0].Split(",")|> Array.toList |> ParseWire
  let wire2 = lines.[1].Split(",")|> Array.toList |> ParseWire

  let minDistance = FindFastestIntersection wire1 wire2

  Assert.Equal(15612, minDistance)
