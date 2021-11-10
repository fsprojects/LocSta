#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module BaseTests

open FsUnit
open FsLocalState
open FsLocalState.Lib.Gen
open NUnit.Framework

let [<TestCase>] ``Pairwise`` () =
    loop {
        let! v1 = Gen.ofList [ "a"; "b"; "c"; "d" ]
        let! v2 = Gen.ofList [  1 ;  2 ;  3 ;  4  ]
        return Loop.Emit (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]

let [<TestCase>] ``Pairwise using For Loop`` () =
    loop {
        for v1 in [ "a"; "b"; "c"; "d" ] do
        for v2 in [  1 ;  2 ;  3 ;  4  ] do
            return Loop.Emit (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]

let [<TestCase>] ``Zero (gen)`` () =
    loop {
        let! v = [ 0; 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            return Loop.Emit v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]

let [<TestCase>] ``Zero For Loop (gen)`` () =
    loop {
        for v in [ 0; 1; 2; 3; 4; 5; 6 ] do
            if v % 2 = 0 then
                return Loop.Emit v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]


let [<TestCase>] ``Stop after Emit`` () =
    let expect = 3
    loop {
        let! c = count 0 1
        if c = expect then
            return Loop.Emit c
            return Loop.Stop
    }
    |> Gen.toList
    |> List.exactlyOne
    |> should equal expect

let [<TestCase>] ``Feedback: both binds + discard`` () =
    feed {
        let! state = Init 0
        let! c1 = count 0 10
        let! c2 = count 0 3
        let currValue = state + c1 + c2
        let nextValue = state + 1
        if currValue % 2 = 0 then
            return Feed.Feedback (currValue, nextValue)
        else
            return Feed.DiscardWith nextValue
    }
    |> Gen.toListn 4
    |> should equal [ (0 + 0 + 0); (1 + 10 + 3); (2 + 20 + 6); (3 + 30 + 9) ]
    
let [<TestCase>] ``Stop (gen)`` () =
    loop {
        let! v = count 0 1
        if v < 5 then
            return Loop.Emit v
        else
            return Loop.Stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]

let [<TestCase>] ``Stop (fdb)`` () =
    feed {
        let! v = Init 0
        let nextValue = v + 1
        if v < 5 then
            return Feed.Feedback(v, nextValue)
        else
            return Feed.Stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]

////let [<TestCase>] ``Reset (gen)`` () =
////    loop {
////        let! prove = count 0 1
////        let! v = loop {
////            let! c1 = count 0 1
////            let! c2 = count 0 1
////            if c1 = 3 && c2 = 3 then
////                return Control.Reset
////            else
////                return Loop.EmitAndLoop c
////        }
////        return Loop.EmitAndLoop (prove, v)
////    }
////    |> Gen.toListn 9
////    |> should equal 
////        [
////            0,0; 1,1; 2,2
////            3,0; 4,1; 5,2
////            6,0; 7,1; 8,2
////        ]

let [<TestCase>] ``Singleton`` () =
    loop {
        return Loop.Emit 0
        return Loop.Stop
    }
    |> Gen.toList
    |> should equal [ 0 ]


let [<TestCase>] ``Combine`` () =
    loop {
        return Loop.Emit 0
        return Loop.Emit 1
        return Loop.Emit 2
        return! Gen.ofList [ 3; 4; 5 ]
        return Loop.Emit 6
        return! Gen.ofList [ 7; 8; 9 ]
        return Loop.Emit 10
        return Loop.Emit 11
    }
    |> Gen.toList
    |> should equal
        [
            0; 1; 2; 3; 6; 7; 10; 11
            0; 1; 2; 4; 6; 8; 10; 11
            0; 1; 2; 5; 6; 9; 10; 11
            0; 1; 2
        ]
