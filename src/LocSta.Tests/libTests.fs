#if INTERACTIVE
#r "../LocSta/bin/Debug/netstandard2.0/LocSta.dll"
open LocSta
open LocSta.Lib.Gen
#endif

module ``Lib Tests``

open TestHelper
open FsUnit
open LocSta
open LocSta.Lib.Gen
open NUnit.Framework


let [<TestCase>] ``Function: whenFuncThenReset`` () =
    count 0 1
    |> whenFuncThenReset (fun v -> v = 5)
    |> Gen.toListn (5 * 3)
    |> equals
        [ 
            0; 1; 2; 3; 4
            0; 1; 2; 3; 4
            0; 1; 2; 3; 4 
        ]


let [<TestCase>] ``Function: onStopThenReset`` () =
    [0..2]
    |> Gen.ofListOneByOne
    |> onStopThenReset
    |> Gen.toListn 9
    |> equals
        [ 
            0; 1; 2 
            0; 1; 2 
            0; 1; 2 
        ]


let [<TestCase>] ``Function: count`` () =
    count 0 1
    |> Gen.toListn 5
    |> equals [0..4]


let [<TestCase>] ``Function: countToCyclic`` () =
    countToCyclic 0 1 3
    |> Gen.toListn 12
    |> equals
        [
            yield! [0..3]
            yield! [0..3]
            yield! [0..3]
        ]


let [<TestCase>] ``Function: ofListOneByOne`` () =
    loop {
        yield 0
        yield 1
        yield 2
        return! Gen.ofListOneByOne [ 3; 4; 5 ]
        yield 6
        return! Gen.ofListOneByOne [ 7; 8; 9 ]
        yield 10
        yield 11
    }
    |> Gen.toList
    |> equals
        [
            0; 1; 2; 3; 6; 7; 10; 11
            0; 1; 2; 4; 6; 8; 10; 11
            0; 1; 2; 5; 6; 9; 10; 11
            0; 1; 2
        ]


let [<TestCase>] ``Function: ofListAllAtOnce`` () =
    loop {
        yield 0
        yield 1
        yield 2
        return! Gen.ofListAllAtOnce [ 3; 4; 5 ]
        yield 6
        return! Gen.ofListAllAtOnce [ 7; 8; 9 ]
        yield 10
        yield 11
    }
    |> Gen.toListn 24
    |> equals
        [
            0; 1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11
            0; 1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11
        ]


let [<TestCase>] ``Function: ofOneTimeValue`` () =
    Gen.ofOneTimeValue 42
    |> Gen.toList
    |> equals [42]


let [<TestCase>] ``GetSlice`` () =
    [0..9]
    |> Gen.ofListOneByOne
    |> fun g -> g.[3..5]
    |> Gen.toList
    |> equals [3;4;5]

//let [<TestCase>] ``Accumulate onc part`` () =
//    loop {
//        for x in [0..10] do
//            let! values = accumulateOnePart 3 x
//            yield values
//    }
//    |> Gen.toList
//    |> equals
//        [
//            [ 0; 1; 2 ]
//        ]


//let [<TestCase>] ``Accumulate many parts`` () =
//    loop {
//        for x in [0..10] do
//            let! values = accumulateManyParts 3 x
//            yield values
//    }
//    |> Gen.toList
//    |> equals
//        [
//            [ 0; 1; 2 ]
//            [ 3; 4; 5 ]
//            [ 6; 7; 8 ]
//        ]


let [<TestCase>] ``Function: onStopThenDefault`` () =
    let defaultValue = 42
    loop {
        let g = [0..3] |> Gen.ofListOneByOne
        let! v = g |> Gen.onStopThenDefault defaultValue
        yield v
    }
    |> Gen.toListn 10
    |> equals
        [ 
            0; 1; 2; 3
            defaultValue; defaultValue; defaultValue; defaultValue; defaultValue; defaultValue
        ]


let [<TestCase>] ``Functions: Skip and Take`` () =
    Gen.ofListOneByOne [ 0.. 10 ]
    |> skip 2 
    |> take 4
    |> Gen.toList
    |> equals [2..5]


//let [<TestCase>] ``Fork`` () =
//    // Task: accumulate value and 2 successors
//    loop {
//        let! v = Gen.ofList [ 0.. 10 ]
//        let! x = fork <| feed {
//            let! state = Init []
//            let newState = v :: state
//            let! c = count 0 1
//            if c = 2 then
//                // TODO: StopWith wäre schon cool, weil der State vor Stop irrelevant ist.
//                yield newState |> List.rev, newState
//                return Feed.Stop
//            return Feed.SkipWith newState
//        }
//        yield x
//        //return! accumulateOnePart 3 v |> fork
//    }
//    |> Gen.toList
//    |> equals
//        [
//            [0; 1; 2]; [1; 2; 3]; [2; 3; 4]
//            [3; 4; 5]; [4; 5; 6]; [5; 6; 7]
//            [6; 7; 8]; [7; 8; 9]; [8; 9; 10]
//        ]
