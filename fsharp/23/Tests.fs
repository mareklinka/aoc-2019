module Tests
open Xunit

open IntComputer
open System.Collections.Generic

module Queue =
    let dequeue<'a> (q: Queue<'a>) = q.Dequeue()

    let count<'a> (q: Queue<'a>) = q.Count

    let enqueue<'a> (v: 'a) (q: Queue<'a>) = q.Enqueue(v)

    let enqueueAll<'a> (v: seq<'a>) (q: Queue<'a>) = v |> Seq.iter q.Enqueue

let rec runNetwork inputQueues (outputQueues: Map<int, ResizeArray<int64>>) programStates =
    let mutable natY = None

    let stopper = Some(fun _ -> true)

    let readerFunction addr =
        fun () ->
            let iq = inputQueues |> Map.find addr
            match iq |> Queue.count with
            | 0 -> -1L
            | _ -> iq |> Queue.dequeue

    let writerFunction addr =
        fun value ->
            let ob = outputQueues |> Map.find addr
            match ob.Count with
            | 2 ->
                match ob.[0] with
                | 255L -> natY <- Some(value)
                | _ ->
                    let iq = inputQueues |> Map.find (ob.[0] |> int)
                    iq.Enqueue ob.[1]
                    iq.Enqueue value
                ob.Clear()
            | _ -> ob.Add value

    let newStates =
        { 0 .. 49 }
        |> Seq.fold (fun acc addr ->
            match natY with
            | Some(_) -> acc
            | None ->
                let { Memory = m; InstructionPointer = ip; RelativeBase = rb; FullStop = _ } =
                    programStates |> Map.find addr

                let newCs = m |> ExecuteProgram ip rb (readerFunction addr) (writerFunction addr) stopper
                acc |> Map.add addr newCs) Map.empty

    match natY with
    | Some(v) -> v
    | None -> runNetwork inputQueues outputQueues newStates

let rec runNetworkWithNat inputQueues (outputQueues: Map<int, ResizeArray<int64>>) latestNatPackage
        lastDeliveredNatY idleCount programStates =
    let mutable nextNatPackage = latestNatPackage
    let mutable hasWriteOccurred = false
    let mutable hasReadOccurred = false

    let stopper = Some(fun _ -> true)

    let readerFunction addr =
        fun () ->
            let iq = inputQueues |> Map.find addr
            match iq |> Queue.count with
            | 0 -> -1L
            | _ ->
                hasReadOccurred <- true
                iq |> Queue.dequeue

    let writerFunction addr =
        fun value ->
            let ob = outputQueues |> Map.find addr
            match ob.Count with
            | 2 ->
                match ob.[0] with
                | 255L -> nextNatPackage <- (ob.[1], value)
                | _ ->
                    hasWriteOccurred <- true
                    let iq = inputQueues |> Map.find (ob.[0] |> int)
                    iq.Enqueue ob.[1]
                    iq.Enqueue value
                ob.Clear()
            | _ -> ob.Add value

    let newStates =
        { 0 .. 49 }
        |> Seq.fold (fun acc addr ->
            let { Memory = m; InstructionPointer = ip; RelativeBase = rb; FullStop = _ } =
                programStates |> Map.find addr

            let newCs = m |> ExecuteProgram ip rb (readerFunction addr) (writerFunction addr) stopper
            acc |> Map.add addr newCs) Map.empty

    match hasReadOccurred || hasWriteOccurred with
    | true ->
        // not idling
        runNetworkWithNat inputQueues outputQueues nextNatPackage lastDeliveredNatY 0 newStates
    | false ->
        match idleCount with
        | 700 ->
            match (nextNatPackage |> snd) = lastDeliveredNatY with
            | true -> nextNatPackage |> snd
            | false ->
                // idling for long enough - send the NAT package to 0
                let iq = inputQueues |> Map.find 0
                iq |> Queue.enqueue (nextNatPackage |> fst)
                iq |> Queue.enqueue (nextNatPackage |> snd)

                runNetworkWithNat inputQueues outputQueues nextNatPackage (nextNatPackage |> snd) 0 newStates
        | _ ->
            // not idling long enough - continue execution normally
            runNetworkWithNat inputQueues outputQueues nextNatPackage lastDeliveredNatY (idleCount + 1) newStates

[<Fact>]
let Part1() =
    let program = parseProgram "input.txt"

    let inputQueues =
        { 0 .. 49 }
        |> Seq.fold (fun acc c ->
            let q = Queue<int64>()
            q |> Queue.enqueue (c |> int64)
            acc |> Map.add c q) Map.empty

    let outputBuffers = { 0 .. 49 } |> Seq.fold (fun acc c -> acc |> Map.add c (ResizeArray())) Map.empty

    let programStates =
        { 0 .. 49 }
        |> Seq.fold (fun acc c ->
            acc
            |> Map.add c
                   { Memory = program |> Array.copy
                     InstructionPointer = 0
                     RelativeBase = 0
                     FullStop = false }) Map.empty

    let y = runNetwork inputQueues outputBuffers programStates

    Assert.Equal(21089L, y)

[<Fact>]
let Part2() =
    let program = parseProgram "input.txt"

    let inputQueues =
        { 0 .. 49 }
        |> Seq.fold (fun acc c ->
            let q = Queue<int64>()
            q |> Queue.enqueue (c |> int64)
            acc |> Map.add c q) Map.empty

    let outputBuffers = { 0 .. 49 } |> Seq.fold (fun acc c -> acc |> Map.add c (ResizeArray())) Map.empty

    let programStates =
        { 0 .. 49 }
        |> Seq.fold (fun acc c ->
            acc
            |> Map.add c
                   { Memory = program |> Array.copy
                     InstructionPointer = 0
                     RelativeBase = 0
                     FullStop = false }) Map.empty

    let y = runNetworkWithNat inputQueues outputBuffers (-1L, -1L) -1L 0 programStates

    Assert.Equal(16658L, y)