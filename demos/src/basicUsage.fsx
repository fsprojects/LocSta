
#r "../../src/FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"

open System
open FsLocalState

[<Struct>]
type Point<'a> = { value: 'a; time: DateTime }

let inline accu windowSize (input: 'a) =
    [] |> Gen.ofSeed (fun state ->
        let state = (input :: state) |> List.truncate windowSize
        let newValue = state |> List.sum
        Some (newValue, state)
    )

let inline add amount (input: 'a) =
    () |> Gen.ofSeed (fun state ->
        let state = ()
        let newValue = input + amount
        Some (newValue, state)
    )

let inline dy (input: 'a) =
    input |> Gen.ofSeed (fun last ->
        let diff = input - last
        Some (diff, input)
    )

let inline dyRel (input: float) =
    input |> Gen.ofSeed (fun last ->
        let diff = input / last - 1.0
        Some (diff, input)
    )

let inline dyFromStart (input: float) =
    input |> Gen.ofSeed (fun start ->
        let diff = input - start
        Some (diff, start)
    )

let inline dyRelFromStart (input: float) =
    input |> Gen.ofSeed (fun start ->
        let diff = input / start - 1.0
        Some (diff, start)
    )

let inline forValueOnly inner (input: 'a Point) =
    gen {
        let! x = inner input.value
        return { input with value = x }
    }

let mapTest =
    [ 1;2;3 ]
    |> Gen.ofList
    |> Gen.map (fun x -> x + 10)
    |> Gen.toList
