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
            yield values
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
            yield values
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
        let! v = g |> Gen.onStopThenDefault defaultValue
        yield v
    }
    |> Gen.toListn 10
    |> should equal
        [ 
            0; 1; 2; 3
            defaultValue; defaultValue; defaultValue; defaultValue; defaultValue; defaultValue
        ]


let [<TestCase>] ``Skip and Take`` () =
    Gen.ofList [ 0.. 10 ]
    |> skip 2 
    |> take 4
    |> Gen.toList
    |> should equal [2..5]


let [<TestCase>] ``Fork`` () =
    // Task: accumulate value and 2 successors

    loop {
        let! v = Gen.ofList [ 0.. 10 ]
        let! x = fork <| feed {
            let! state = Init []
            let newState = v :: state
            let! c = count 0 1
            if c = 2 then
                // TODO: ValueThenStop wäre schon cool, weil der State vor Stop irrelevant ist.
                yield newState |> List.rev, newState
                return Feed.Stop
            return Feed.SkipWith newState
        }
        yield x
        //return! accumulateOnePart 3 v |> fork
    }
    |> Gen.toList
    |> should equal
        [[[0; 1; 2]]; [[1; 2; 3]]; [[2; 3; 4]]; [[3; 4; 5]]; [[4; 5; 6]];
         [[5; 6; 7]]; [[6; 7; 8]]; [[7; 8; 9]]; [[8; 9; 10]]]
