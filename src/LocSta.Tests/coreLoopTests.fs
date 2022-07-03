#if INTERACTIVE
#r "../LocSta/bin/Debug/netstandard2.0/LocSta.dll"
#else
module ``Core Tests (Loop)``

open NUnit.Framework
open TestHelper
#endif

open LocSta


let [<TestCase>] ``yield (repeating single value)`` () =
    loop { 5 }
    |> Gen.toListn 10
    |> equals [ 5; 5; 5; 5; 5; 5; 5; 5; 5; 5 ]


let [<TestCase>] ``Loop.Emit (repeating single value)`` () =
    loop {
        return Loop.Emit 5
    }
    |> Gen.toListn 10
    |> equals [ 5; 5; 5; 5; 5; 5; 5; 5; 5; 5 ]


let [<TestCase>] ``yield! (repeating many values in sequence)`` () =
    loop {
        yield! [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6; 5; 6; 5; 6; 5; 6; 5; 6 ]


let [<TestCase>] ``Loop.EmitMany (repeating many values in sequence)`` () =
    loop {
        return Loop.EmitMany [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6; 5; 6; 5; 6; 5; 6; 5; 6 ]


let [<TestCase>] ``Loop.EmitAndStop (one-time value)`` () =
    loop {
        return Loop.EmitAndStop 5
    }
    |> Gen.toListn 10
    |> equals [ 5 ]


let [<TestCase>] ``Loop.EmitManyAndStop (collected list of one-time values)`` () =
    loop {
        return Loop.EmitManyAndStop [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6 ]


let [<TestCase>] ``Loop.Stop (no value / empty result)`` () =
    loop {
        return Loop.Stop
    }
    |> Gen.toListn 10
    |> equals []


// TODO: Combine scenarios with Loop.EmitAndReset and other reset functions


let [<TestCase>] ``Loop.EmitAndReset`` () =
    loop {
        let! c = Gen.count 0 1
        match c = 3 with
        | true -> return Loop.EmitAndReset c
        | false -> yield c
    }
    |> Gen.toListn 12
    |> equals
        [
            0; 1; 2; 3
            0; 1; 2; 3
            0; 1; 2; 3
        ]


let [<TestCase>] ``Loop.EmitManyAndReset`` () =
    loop {
        let! c = Gen.count 0 1
        match c = 3 with
        | true -> return Loop.EmitManyAndReset [ 66; 77 ]
        | false -> yield c
    }
    |> Gen.toListn 15
    |> equals
        [
            0; 1; 2; 66; 77
            0; 1; 2; 66; 77
            0; 1; 2; 66; 77
        ]

    
let [<TestCase>] ``Loop.SkipAndReset`` () =
    loop {
        let! c = Gen.count 0 1
        match c = 3 with
        | true -> return Loop.SkipAndReset
        | false -> yield c
    }
    |> Gen.toListn 9
    |> equals
        [
            0; 1; 2
            0; 1; 2
            0; 1; 2
        ]


let [<TestCase>] ``Loop.Skip`` () =
    loop {
        let! c = Gen.count 0 1
        match c <> 5 with
        | true -> c
        | false -> return Loop.Skip
    }
    |> Gen.toListn 10
    |> equals [ 0; 1; 2; 3; 4; (* skip 5 *) 6; 7; 8; 9; 10 ]


let [<TestCase>] ``Zero (implicit skip)`` () =
    loop {
        let! c = Gen.count 0 1
        if c <> 5 then c
    }
    |> Gen.toListn 10
    |> equals [ 0; 1; 2; 3; 4; (* skip 5 *) 6; 7; 8; 9; 10 ]
    

let [<TestCase>] ``Combine: yield, yield, yield`` () =
    loop {
        yield 5
        yield 6
        yield 7
    }
    |> Gen.toListn 9
    |> equals [ 5; 6; 7;   5; 6; 7;   5; 6; 7 ]


let [<TestCase>] ``Combine: let!, let! yield, let!, let! yield, let!, let! yield`` () =
    loop {
        let! c11 = Gen.count 0 1
        let! c12 = Gen.count 5 1
        yield c11 + c12

        let! c21 = Gen.count 10 1
        let! c22 = Gen.count 15 1
        yield c21 + c22

        let! c31 = Gen.count 20 1
        let! c32 = Gen.count 25 1
        yield c31 + c32
    }
    |> Gen.toListn 9
    |> equals [ 5; 25; 45;   7; 27; 47;   9; 29; 49 ]


let [<TestCase>] ``Combine: let!, yield, let!, yield, Loop.Stop, yield`` () =
    loop {
        let! c = Gen.count 0 1
        yield c

        let! c = Gen.count 10 1
        yield c

        return Loop.Stop

        yield 100
    }
    |> Gen.toListn 10
    |> equals [ 0; 10 ]


let [<TestCase>] ``For`` () =
    loop {
        for v in [ 0; 1; 2; 3 ] do
            yield v
    }
    |> Gen.toListn 8
    |> equals [ 0; 1; 2; 3;   0; 1; 2; 3; ]


let [<TestCase>] ``Combine: For, let!, For`` () =
    loop {
        for v in [ 0; 1; 2; 3 ] do
            yield v

        let! c = Gen.count 10 10
        for v in [ 6; 7; 8; 9 ] do
            yield v + c
    }
    |> Gen.toListn 16
    |> equals [ 0; 1; 2; 3;   16; 17; 18; 19;   0; 1; 2; 3;   26; 27; 28; 29 ]


let [<TestCase>] ``Combine: For, Zero, let!, For, Zero`` () =
    loop {
        for v in [ 0; 1; 2; 3 ] do
            if v % 2 = 0 then
                yield v

        let! c = Gen.count 10 10
        for v in [ 4; 5; 6; 7 ] do
            if v % 2 = 0 then
                yield v + c
    }
    |> Gen.toListn 8
    |> equals [ 0; 2;   14; 16;   0; 2;   24; 26 ]
