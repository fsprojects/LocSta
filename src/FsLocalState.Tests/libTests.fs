#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module LibTests

open FsUnit
open FsLocalState
open FsLocalState.Lib.Gen
open NUnit.Framework


let [<TestCase>] ``Reset by current`` () =
    count 0 1
    |> whenFuncThenReset (fun v -> v = 5)
    |> Gen.toListn (5 * 3)
    |> should equal
        [ 
            0; 1; 2; 3; 4
            0; 1; 2; 3; 4
            0; 1; 2; 3; 4 
        ]

let [<TestCase>] ``Reset on stop`` () =
    [0..2]
    |> Gen.ofList
    |> onStopThenReset
    |> Gen.toListn 9
    |> should equal
        [ 
            0; 1; 2 
            0; 1; 2 
            0; 1; 2 
        ]

let [<TestCase>] ``Count 0 1`` () =
    count 0 1
    |> Gen.toListn 5
    |> should equal [0..4]

let [<TestCase>] ``Count until repeat`` () =
    repeatCount 0 1 3
    |> Gen.toListn 12
    |> should equal
        [
            yield! [0..3]
            yield! [0..3]
            yield! [0..3]
        ]

let [<TestCase>] ``Accumulate onc part`` () =
    loop {
        for x in [0..10] do
            let! values = accumulateOnePart 3 x
            return Loop.Emit values
    }
    |> Gen.toList
    |> should equal
        [
            [ 0; 1; 2 ]
        ]

let [<TestCase>] ``Accumulate many parts`` () =
    loop {
        for x in [0..10] do
            let! values = accumulateManyParts 3 x
            return Loop.Emit values
    }
    |> Gen.toList
    |> should equal
        [
            [ 0; 1; 2 ]
            [ 3; 4; 5 ]
            [ 6; 7; 8 ]
        ]


let [<TestCase>] ``Default on Stop`` () =
    let defaultValue = 42
    loop {
        let g = [0..3] |> Gen.ofList
        let! v = g |> Gen.defaultOnStop defaultValue
        return Loop.Emit v
    }
    |> Gen.toListn 10
    |> should equal
        [ 
            0; 1; 2; 3
            defaultValue; defaultValue; defaultValue; defaultValue; defaultValue; defaultValue
        ]

let [<TestCase>] ``Fork`` () =
    // Task: accumulate value and 2 successors

    loop {
        let! v = Gen.ofList [ 0.. 10 ]
        return! accumulateOnePart 3 v |> fork
    }
    |> Gen.toListn 9
    |> List.last
    |> should equal
        [
            [ 8; 7; 6 ]
            [ 5; 4; 3 ]
            [ 2; 1; 0 ]
        ]
