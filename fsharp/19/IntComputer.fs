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

let WrapInput(list: int64 list) =
    let mutable state = -1
    fun () ->
        state <- state + 1
        list.[state]

let parseProgram path =
    let program = ref (File.ReadAllText(path).Split(",") |> Array.map int64)
    array.Resize(program, 1024 * 10) // 10kB
    !program

let private readParameterMode address position (program: int64 array) =
    let mode = (program.[address] / ((pown 10 (2 + position)) |> int64)) % 10L
    match mode with
    | 0L -> Address
    | 1L -> Immediate
    | 2L -> Relative

let private readParameter (program: int64 array) address relativeBase mode =
    match mode with
    | Address -> program.[address |> int]
    | Immediate -> address
    | Relative -> program.[(relativeBase + (address |> int))]

let private getWritePosition address relativeBase (mode: ParameterMode) =
    match mode with
    | Address -> address |> int
    | Relative -> relativeBase + (address |> int)

let rec ExecuteProgram instructionPointer relativeBase reader writer earlyStopper (program: int64 array) =
    let instruction = program.[instructionPointer] % 100L

    let newPointer =
        match instruction with
        | 1L ->
            let a =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase

            let b =
                program
                |> readParameterMode instructionPointer 1
                |> readParameter program program.[instructionPointer + 2] relativeBase

            let p =
                program
                |> readParameterMode instructionPointer 2
                |> getWritePosition program.[instructionPointer + 3] relativeBase

            program.[p] <- a + b

            Some(instructionPointer + 4), None
        | 2L ->
            let a =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase

            let b =
                program
                |> readParameterMode instructionPointer 1
                |> readParameter program program.[instructionPointer + 2] relativeBase

            let p =
                program
                |> readParameterMode instructionPointer 2
                |> getWritePosition program.[instructionPointer + 3] relativeBase

            program.[p] <- a * b

            Some(instructionPointer + 4), None
        | 3L ->
            let p =
                program
                |> readParameterMode instructionPointer 0
                |> getWritePosition program.[instructionPointer + 1] relativeBase
            program.[p] <- reader()

            Some(instructionPointer + 2), None
        | 4L ->
            let a =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase
            writer (a)

            Some(instructionPointer + 2), None
        | 5L ->
            let value =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase

            let target =
                program
                |> readParameterMode instructionPointer 1
                |> readParameter program program.[instructionPointer + 2] relativeBase

            match value with
            | 0L -> Some(instructionPointer + 3), None
            | _ -> Some(target |> int), None
        | 6L ->
            let value =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase

            let target =
                program
                |> readParameterMode instructionPointer 1
                |> readParameter program program.[instructionPointer + 2] relativeBase

            match value with
            | 0L -> Some(target |> int), None
            | _ -> Some(instructionPointer + 3), None
        | 7L ->
            let a =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase

            let b =
                program
                |> readParameterMode instructionPointer 1
                |> readParameter program program.[instructionPointer + 2] relativeBase

            let p =
                program
                |> readParameterMode instructionPointer 2
                |> getWritePosition program.[instructionPointer + 3] relativeBase

            program.[p] <- if a < b then 1L else 0L

            Some(instructionPointer + 4), None
        | 8L ->
            let a =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase

            let b =
                program
                |> readParameterMode instructionPointer 1
                |> readParameter program program.[instructionPointer + 2] relativeBase

            let p =
                program
                |> readParameterMode instructionPointer 2
                |> getWritePosition program.[instructionPointer + 3] relativeBase

            program.[p] <- if a = b then 1L else 0L

            Some(instructionPointer + 4), None
        | 9L ->
            let a =
                program
                |> readParameterMode instructionPointer 0
                |> readParameter program program.[instructionPointer + 1] relativeBase

            Some(instructionPointer + 2), Some(relativeBase + (a |> int))
        | 99L -> None, None


    let requiresEarlyStop =
        match earlyStopper with
        | Some stopFunction -> instruction |> stopFunction
        | None -> false

    match newPointer with
    | Some ip, Some b ->
        match requiresEarlyStop with
        | true ->
            { ComputerState.Memory = program
              ComputerState.InstructionPointer = ip
              ComputerState.RelativeBase = b
              FullStop = false }
        | false -> program |> ExecuteProgram ip b reader writer earlyStopper
    | Some ip, None ->
        match requiresEarlyStop with
        | true ->
            { ComputerState.Memory = program
              ComputerState.InstructionPointer = ip
              ComputerState.RelativeBase = relativeBase
              FullStop = false }
        | false -> program |> ExecuteProgram ip relativeBase reader writer earlyStopper
    | None, _ ->
        { ComputerState.Memory = program
          ComputerState.InstructionPointer = instructionPointer
          ComputerState.RelativeBase = relativeBase
          ComputerState.FullStop = true }
