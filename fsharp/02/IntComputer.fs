module IntComputer

type ParameterMode = Address | Immediate | Relative  

let ReadParameterMode (program:int64 array) address position =
    let mode = (program.[address] / (int64)(pown 10 2+position)) % 10L
    match mode with
    | 0L -> ParameterMode.Address
    | 1L -> ParameterMode.Immediate
    | 2L -> ParameterMode.Relative

let ReadParameter (program:int64 array) (address:int64) relativeBase mode =
    match mode with
    | ParameterMode.Address -> program.[(int32)address]
    | ParameterMode.Immediate -> address
    | ParameterMode.Relative -> program.[(relativeBase + (int32)address)]

let GetWritePosition address relativeBase (mode:ParameterMode) =
    match mode with
    | ParameterMode.Address -> (int32)address
    | ParameterMode.Relative -> relativeBase + (int32)address

let rec ExecuteProgram (program:int64 array) instructionPointer relativeBase reader writer =
    let instruction = program.[instructionPointer]
    let newPointer = match instruction with 
    | 1L -> 
        let a = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        let b = ReadParameterMode program instructionPointer 1 |> ReadParameter program program.[instructionPointer+2] relativeBase

        let p = ReadParameterMode program instructionPointer 2 |> GetWritePosition program.[instructionPointer+3] relativeBase
        program.[p] <- a + b

        Some(instructionPointer + 4), None
    | 2L -> 
        let a = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        let b = ReadParameterMode program instructionPointer 1 |> ReadParameter program program.[instructionPointer+2] relativeBase

        let p = ReadParameterMode program instructionPointer 2 |> GetWritePosition program.[instructionPointer+3] relativeBase
        program.[p] <- a * b

        Some(instructionPointer + 4),None
    | 3L ->
        let p = ReadParameterMode program instructionPointer 0 |> GetWritePosition program.[instructionPointer+1] relativeBase
        program.[p] <- reader()

        Some(instructionPointer + 2), None
    | 4L ->
        let a = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        writer(a)

        Some(instructionPointer + 2), None
    | 5L -> 
        let value = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        let target = ReadParameterMode program instructionPointer 1 |> ReadParameter program program.[instructionPointer+2] relativeBase

        match value with
        | 0L -> Some(instructionPointer + 3), None
        | _ -> Some((int32)target), None
    | 6L -> 
        let value = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        let target = ReadParameterMode program instructionPointer 1 |> ReadParameter program program.[instructionPointer+2] relativeBase

        match value with
        | 0L -> Some((int32)target),None
        | _ -> Some(instructionPointer + 3),None
    | 7L -> 
        let a = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        let b = ReadParameterMode program instructionPointer 1 |> ReadParameter program program.[instructionPointer+2] relativeBase

        let p = ReadParameterMode program instructionPointer 2 |> GetWritePosition program.[instructionPointer+3] relativeBase
        program.[p] <- if a < b then 1L else 0L

        Some(instructionPointer + 4),None
    | 8L -> 
        let a = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        let b = ReadParameterMode program instructionPointer 1 |> ReadParameter program program.[instructionPointer+2] relativeBase

        let p = ReadParameterMode program instructionPointer 2 |> GetWritePosition program.[instructionPointer+3] relativeBase
        program.[p] <- if a = b then 1L else 0L

        Some(instructionPointer + 4),None
    | 9L ->
        let a = ReadParameterMode program instructionPointer 0 |> ReadParameter program program.[instructionPointer+1] relativeBase
        
        Some(instructionPointer + 2), Some((int32)a)
    | 99L -> None,None

    match newPointer with
    | Some ip, Some b -> ExecuteProgram program ip b reader writer
    | Some ip, None -> ExecuteProgram program ip relativeBase reader writer
    | None, _ -> program