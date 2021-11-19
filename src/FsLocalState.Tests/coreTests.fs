#if INTERACTIVE
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module FsLocalState.CoreTests

open TestHelper
open FsUnit
open FsLocalState
open FsLocalState.Lib.Gen
open NUnit.Framework


// TODO: More systematic testing

let [<TestCase>] ``Yield repeating value (loop)`` () =
    loop {
        yield 5
    }
    |> Gen.toListn 10
    |> equals (List.replicate 10 5)


let [<TestCase>] ``Emit repeating value (loop)`` () =
    loop {
        return Loop.Emit 5
    }
    |> Gen.toListn 10
    |> equals [ 5; 5; 5; 5; 5; 5; 5; 5; 5; 5 ]



let [<TestCase>] ``Emit many repeating values (loop)`` () =
    loop {
        return Loop.EmitMany [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6; 5; 6; 5; 6; 5; 6; 5; 6 ]

    

let [<TestCase>] ``Emit one value (loop)`` () =
    loop {
        return Loop.EmitAndStop 5
    }
    |> Gen.toListn 10
    |> equals [ 5 ]


let [<TestCase>] ``Emit many values one time (loop)`` () =
    loop {
        return Loop.EmitManyAndStop [5;6]
    }
    |> Gen.toListn 10
    |> equals [ 5; 6 ]

    
let [<TestCase>] ``Skip explicit (loop)`` () =
    loop {
        let! c = count 0 1
        if c = 5 then 
            return Loop.Skip
        else
            c
    }
    |> Gen.toListn 10
    |> equals [ 0; 1; 2; 3; 4; (* skip 5 *) 6; 7; 8; 9; 10 ]


let [<TestCase>] ``Skip implicit (loop)`` () =
    loop {
        let! c = count 0 1
        if c <> 5 then c
    }
    |> Gen.toListn 10
    |> equals [ 0; 1; 2; 3; 4; (* skip 5 *) 6; 7; 8; 9; 10 ]


let [<TestCase>] ``Stop (loop)`` () =
    loop {
        return Loop.Stop
    }
    |> Gen.toListn 10
    |> equals []


let [<TestCase>] ``Combine yield + stop (loop)`` () =
    loop {
        let! c = count 0 1
        yield c
        let! c = count 10 1
        yield c
        return Loop.Stop
        yield 100
    }
    |> Gen.toListn 10
    |> equals [0;10]
    

// TODO: how model combine on feed?
//let [<TestCase>] ``TODO`` () =
//    feed {
//        let! state = Init 10
//        yield 5,state
        
//        let! state = Init 10
//        yield 5,state
//    }
//    |> Gen.toListn 10
//    |> equals [0;10]
    