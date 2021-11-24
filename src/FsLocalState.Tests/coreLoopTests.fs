#if INTERACTIVE
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
open FsLocalState.Lib.Gen
#endif

module ``Core Tests (Loop)``

open TestHelper
open FsLocalState
open FsLocalState.Lib.Gen
open NUnit.Framework


let [<TestCase>] ``Repeating single value with 'yield'`` () =
    loop { 5 }
    |> Gen.toListn 10
    |> equals [ 5; 5; 5; 5; 5; 5; 5; 5; 5; 5 ]


let [<TestCase>] ``Repeating single value with 'Loop.Emit'`` () =
    loop {
        return Loop.Emit 5
    }
    |> Gen.toListn 10
    |> equals [ 5; 5; 5; 5; 5; 5; 5; 5; 5; 5 ]


let [<TestCase>] ``Repeating many values in sequence with 'yield!'`` () =
    loop {
        yield! [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6; 5; 6; 5; 6; 5; 6; 5; 6 ]


let [<TestCase>] ``Repeating many values in sequence with 'Loop.Collect'`` () =
    loop {
        return Loop.Collect [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6; 5; 6; 5; 6; 5; 6; 5; 6 ]
    

let [<TestCase>] ``One-time value with 'Loop.EmitAndStop'`` () =
    loop {
        return Loop.EmitAndStop 5
    }
    |> Gen.toListn 10
    |> equals [ 5 ]


let [<TestCase>] ``One-time values with 'Loop.CollectAndStop'`` () =
    loop {
        return Loop.CollectAndStop [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6 ]


let [<TestCase>] ``No value with 'Loop.Stop'`` () =
    loop {
        return Loop.Stop
    }
    |> Gen.toListn 10
    |> equals []


let [<TestCase>] ``Combine constant values with 3 'yield's`` () =
    loop {
        yield 5
        yield 6
        yield 7
    }
    |> Gen.toListn 9
    |> equals [ 5; 6; 7;   5; 6; 7;   5; 6; 7 ]


let [<TestCase>] ``Combine computed values with 3 'yield's`` () =
    loop {
        let! c11 = count 0 1
        let! c12 = count 5 1
        yield c11 + c12

        let! c21 = count 10 1
        let! c22 = count 15 1
        yield c21 + c22

        let! c31 = count 20 1
        let! c32 = count 25 1
        yield c31 + c32
    }
    |> Gen.toListn 9
    |> equals [ 5; 25; 45;  7; 27; 47;  9; 29; 49 ]


let [<TestCase>] ``Combine computed values with 'yield's and 'Loop.Stop'`` () =
    loop {
        let! c = count 0 1
        yield c

        let! c = count 10 1
        yield c

        return Loop.Stop

        yield 100
    }
    |> Gen.toListn 10
    |> equals [ 0; 10 ]


let [<TestCase>] ``Skip (explicit) with 'Loop.Skip'`` () =
    loop {
        let! c = count 0 1
        if c = 5 then 
            return Loop.Skip
        else
            c
    }
    |> Gen.toListn 10
    |> equals [ 0; 1; 2; 3; 4; (* skip 5 *) 6; 7; 8; 9; 10 ]


let [<TestCase>] ``Skip (implicit) with 'Zero'`` () =
    loop {
        let! c = count 0 1
        if c <> 5 then c
    }
    |> Gen.toListn 10
    |> equals [ 0; 1; 2; 3; 4; (* skip 5 *) 6; 7; 8; 9; 10 ]


let [<TestCase>] ``For`` () =
    loop {
        for v in [ 0; 1; 2; 3 ] do
            yield v
    }
    |> Gen.toListn 8
    |> equals [ 0; 1; 2; 3;  0; 1; 2; 3; ]


let [<TestCase>] ``For combined`` () =
    loop {
        for v in [ 0; 1; 2; 3 ] do
            yield v
        for v in [ 6; 7; 8; 9 ] do
            yield v
    }
    |> Gen.toListn 16
    |> equals [ 0; 1; 2; 3;  6; 7; 8; 9;  0; 1; 2; 3;  6; 7; 8; 9 ]


let [<TestCase>] ``For and Skip combined`` () =
    loop {
        for v in [ 0; 1; 2; 3; 4 ] do
            if v % 2 = 0 then
                yield v
        for v in [ 5; 6; 7; 8; 9 ] do
            if v % 2 = 0 then
                yield v
    }
    |> Gen.toListn 10
    |> equals [ 0; 2; 4; 6; 8;  0; 2; 4; 6; 8 ]
