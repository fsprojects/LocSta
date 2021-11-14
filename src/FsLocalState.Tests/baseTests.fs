#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module BaseTests

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
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Pairwise for (loop)`` () =
    loop {
        for v1 in [ "a"; "b"; "c"; "d" ] do
        for v2 in [  1 ;  2 ;  3 ;  4  ] do
            yield v1,v2
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Zero (loop)`` () =
    loop {
        let! v = [ 0; 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            yield v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]


let [<TestCase>] ``Zero For Loop (loop)`` () =
    loop {
        for v in [ 0; 1; 2; 3; 4; 5; 6 ] do
            if v % 2 = 0 then
                yield v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]


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
    |> should equal expect


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
    |> should equal [ (0 + 0 + 0); (1 + 10 + 3); (2 + 20 + 6); (3 + 30 + 9) ]
    

let [<TestCase>] ``Stop (loop)`` () =
    loop {
        let! v = count 0 1
        if v < 5 then
            yield v
        else
            return Loop.Stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]


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
    |> should equal [ 0 .. 4 ]


let [<TestCase>] ``Singleton (loop)`` () =
    Gen.returnValueOnce 42
    |> Gen.toList
    |> should equal [42]


let [<TestCase>] ``GetSlice`` () =
    [0..9]
    |> Gen.ofList
    |> fun g -> g[3..5]
    |> Gen.toList
    |> should equal [3;4;5]


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
    |> should equal
        [
            0; 1; 2; 3; 6; 7; 10; 11
            0; 1; 2; 4; 6; 8; 10; 11
            0; 1; 2; 5; 6; 9; 10; 11
            0; 1; 2
        ]


 //TODO: Document "if" behaviour (also in combination with combine)
let [<TestCase>] ``ResetThis + Combine (feed)`` () =
    feed {
        let! state = Init 1
        let! c = count 10 10
        if state = 4 then
            yield state + c, state + 1
            return Feed.ResetThis
        if state <> 4 then
            yield state + c, state + 1
    }
    |> Gen.toList
    |> should equal
        [
            11; 22; 33

        ]

// TODO: ResetTree
