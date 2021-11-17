#if INTERACTIVE
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module FsLocalState.BaseTests

open TestHelper
open FsUnit
open FsLocalState
open FsLocalState.Lib.Gen
open NUnit.Framework

let [<TestCase>] ``Pairwise let! (loop)`` () =
    loop {
        let! v1 = Gen.ofList [ "a"; "b"; "c"; "d" ]
        let! v2 = Gen.ofList [  1 ;  2 ;  3 ;  4  ]
        yield v1,v2
    }
    |> Gen.toList
    |> equals [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


//let [<TestCase>] ``Pairwise for (loop)`` () =
//    failwith "TODO"
//    loop {
//        for v1 in [ "a"; "b"; "c"; "d" ] do
//        for v2 in [  1 ;  2 ;  3 ;  4  ] do
//            yield v1,v2
//    }
//    |> Gen.toListn 10
//    |> equals [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


//let [<TestCase>] ``Combine + for (loop)`` () =
//    loop {
//        for v1 in [ "a"; "b"; "c"; "d" ] do
//            yield v1
//        for v2 in [  1 ;  2 ;  3 ;  4  ] do
//            yield v2.ToString()
//            yield "X"
//    }
//    |> Gen.toList
//    |> equals [ "a"; "1"; "X"; "b"; "2"; "X"; "c"; "3"; "X"; "d"; "4"; "X" ]


let [<TestCase>] ``Zero (loop)`` () =
    loop {
        let! v = [ 0; 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            yield v
    }
    |> Gen.toList
    |> equals [ 0; 2; 4; 6 ]


//let [<TestCase>] ``Zero For Loop (loop)`` () =
//    loop {
//        for v in [ 0; 1; 2; 3; 4; 5; 6 ] do
//            if v % 2 = 0 then
//                yield v
//    }
//    |> Gen.toList
//    |> equals [ 0; 2; 4; 6 ]


let [<TestCase>] ``Stop after Emit (loop)`` () =
    let expect = 3
    loop {
        let! c = count 0 1
        if c = expect then
            yield c
            return Loop.Stop
    }
    |> Gen.toList
    |> List.exactlyOne
    |> equals expect


let [<TestCase>] ``Binds + skip (feed)`` () =
    feed {
        let! state = Init 0
        let! c1 = count 0 10
        let! c2 = count 0 3
        let currValue = state + c1 + c2
        let nextValue = state + 1
        if currValue % 2 = 0 then
            yield currValue, nextValue
        else
            return Feed.SkipWith nextValue
    }
    |> Gen.toListn 4
    |> equals [ (0 + 0 + 0); (1 + 10 + 3); (2 + 20 + 6); (3 + 30 + 9) ]
    

let [<TestCase>] ``Stop (loop)`` () =
    loop {
        let! v = count 0 1
        if v < 5 then
            yield v
        else
            return Loop.Stop
    }
    |> Gen.toList
    |> equals [ 0 .. 4 ]


let [<TestCase>] ``Stop (feed)`` () =
    feed {
        let! v = Init 0
        let nextValue = v + 1
        if v < 5 then
            yield v, nextValue
        else
            return Feed.Stop
    }
    |> Gen.toList
    |> equals [ 0 .. 4 ]


let [<TestCase>] ``Singleton`` () =
    Gen.singleton 42
    |> Gen.toList
    |> equals [42]


let [<TestCase>] ``GetSlice`` () =
    [0..9]
    |> Gen.ofList
    |> fun g -> g[3..5]
    |> Gen.toList
    |> equals [3;4;5]


let [<TestCase>] ``Combine (loop)`` () =
    loop {
        yield 0
        yield 1
        yield 2
        return! Gen.ofList [ 3; 4; 5 ]
        yield 6
        return! Gen.ofList [ 7; 8; 9 ]
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


 //TODO: Document "if" behaviour (also in combination with combine)
let [<TestCase>] ``ResetThis + Combine`` () =
    let n = 3
    feed {
        let! state = Init 1
        let! c = count 10 10
        let vf = state + c, state + 1
        if state = n then
            yield vf
            return Feed.ResetThis
        if state <> n then
            yield vf
    }
    |> Gen.toListn 9
    |> equals
        [
            11; 22; 33
            41; 52; 63
            71; 82; 93
        ]

// TODO: ResetTree


 //TODO: Document "if" behaviour (also in combination with combine)
let [<TestCase>] ``Collect`` () =
    failwith "TODO"
    feed {
        let! state = Init 1
        let! c = count 10 10
        let vf = state + c, state + 1
        if state = 4 then
            yield vf
            return Feed.ResetThis
        if state <> 4 then
            yield vf
    }
    |> Gen.toListn 9
    |> equals
        [
            11; 22; 33
            41; 52; 63
            71; 82; 93
        ]
