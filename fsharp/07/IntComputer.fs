module IntComputer

open System.IO

type ParameterMode =
    | Address
    | Immediate
    | Relative

type ComputerState =
    { Memory: int64 array
      InstructionPointer: int
      RelativeBase: int
      FullStop: bool }

let ParseProgram path = File.ReadAllText(path).Split(",") |> Array.map int64

let ReadParameterMode (program: int64 array) address position =
    let mode = (program.[address] / ((pown 10 (2 + position)) |> int64)) % 10L
    match mode with
    | 0L -> Address
    | 1L -> Immediate
    | 2L -> Relative

let ReadParameter (program: int64 array) (address: int64) relativeBase mode =
    match mode with
    | Address -> program.[address |> int32]
    | Immediate -> address
    | Relative -> program.[(relativeBase + (address |> int32))]

let GetWritePosition address relativeBase (mode: ParameterMode) =
    match mode with
    | Address -> address |> int32
    | Relative -> relativeBase + (address |> int32)

let rec ExecuteProgram instructionPointer relativeBase reader writer earlyStopper (program: int64 array) =
    let instruction = program.[instructionPointer] % 100L

    let newPointer =
        match instruction with
        | 1L ->
            let a =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase
            let b =
                ReadParameterMode program instructionPointer 1
                |> ReadParameter program program.[instructionPointer + 2] relativeBase

            let p =
                ReadParameterMode program instructionPointer 2
                |> GetWritePosition program.[instructionPointer + 3] relativeBase
            program.[p] <- a + b

            Some(instructionPointer + 4), None
        | 2L ->
            let a =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase
            let b =
                ReadParameterMode program instructionPointer 1
                |> ReadParameter program program.[instructionPointer + 2] relativeBase

            let p =
                ReadParameterMode program instructionPointer 2
                |> GetWritePosition program.[instructionPointer + 3] relativeBase
            program.[p] <- a * b

            Some(instructionPointer + 4), None
        | 3L ->
            let p =
                ReadParameterMode program instructionPointer 0
                |> GetWritePosition program.[instructionPointer + 1] relativeBase
            program.[p] <- reader()

            Some(instructionPointer + 2), None
        | 4L ->
            let a =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase
            writer (a)

            Some(instructionPointer + 2), None
        | 5L ->
            let value =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase
            let target =
                ReadParameterMode program instructionPointer 1
                |> ReadParameter program program.[instructionPointer + 2] relativeBase

            match value with
            | 0L -> Some(instructionPointer + 3), None
            | _ -> Some(target |> int32), None
        | 6L ->
            let value =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase
            let target =
                ReadParameterMode program instructionPointer 1
                |> ReadParameter program program.[instructionPointer + 2] relativeBase

            match value with
            | 0L -> Some(target |> int32), None
            | _ -> Some(instructionPointer + 3), None
        | 7L ->
            let a =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase
            let b =
                ReadParameterMode program instructionPointer 1
                |> ReadParameter program program.[instructionPointer + 2] relativeBase

            let p =
                ReadParameterMode program instructionPointer 2
                |> GetWritePosition program.[instructionPointer + 3] relativeBase
            program.[p] <- if a < b then 1L else 0L

            Some(instructionPointer + 4), None
        | 8L ->
            let a =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase
            let b =
                ReadParameterMode program instructionPointer 1
                |> ReadParameter program program.[instructionPointer + 2] relativeBase

            let p =
                ReadParameterMode program instructionPointer 2
                |> GetWritePosition program.[instructionPointer + 3] relativeBase
            program.[p] <- if a = b then 1L else 0L

            Some(instructionPointer + 4), None
        | 9L ->
            let a =
                ReadParameterMode program instructionPointer 0
                |> ReadParameter program program.[instructionPointer + 1] relativeBase

            Some(instructionPointer + 2), Some(a |> int)
        | 99L -> None, None

    match earlyStopper with
    | Some f ->
        match f(instruction) with
        | true ->
            match newPointer with
            | Some ip, _ ->
                { ComputerState.Memory = program
                  ComputerState.InstructionPointer = ip
                  ComputerState.RelativeBase = relativeBase
                  FullStop = false }
            | None, _ -> failwith "How did I get here?"
        | false ->
            match newPointer with
            | Some ip, Some b -> program |> ExecuteProgram ip b reader writer earlyStopper
            | Some ip, None -> program |> ExecuteProgram ip relativeBase reader writer earlyStopper
            | None, _ ->
                { ComputerState.Memory = program
                  ComputerState.InstructionPointer = instructionPointer
                  ComputerState.RelativeBase = relativeBase
                  FullStop = true }
    | None ->
        match newPointer with
        | Some ip, Some b -> program |> ExecuteProgram ip b reader writer earlyStopper
        | Some ip, None -> program |> ExecuteProgram ip relativeBase reader writer earlyStopper
        | None, _ ->
            { ComputerState.Memory = program
              ComputerState.InstructionPointer = instructionPointer
              ComputerState.RelativeBase = relativeBase
              ComputerState.FullStop = true }
 