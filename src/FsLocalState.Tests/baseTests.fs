#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module BaseTests

open FsUnit
open FsLocalState
open NUnit.Framework

let [<TestCase>] ``Pairwise`` () =
    gen {
        let! v1 = Gen.ofList [ "a"; "b"; "c"; "d" ]
        let! v2 = Gen.ofList [  1 ;  2 ;  3 ;  4  ]
        return Control.Emit (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]

let [<TestCase>] ``Pairwise using For Loop`` () =
    gen {
        for v1 in [ "a"; "b"; "c"; "d" ] do
        for v2 in [  1 ;  2 ;  3 ;  4  ] do
            return Control.Emit (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]

let [<TestCase>] ``Zero (gen)`` () =
    gen {
        let! v = [ 0; 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            return Control.Emit v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]

let [<TestCase>] ``Zero For Loop (gen)`` () =
    gen {
        for v in [ 0; 1; 2; 3; 4; 5; 6 ] do
            if v % 2 = 0 then
                return Control.Emit v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]

let [<TestCase>] ``Feedback: both binds + discard`` () =
    fdb {
        let! state = Init 0
        let! c = count 10 10
        let currValue = state + c
        let nextValue = state + 1
        if currValue % 2 = 0 then
            return Control.Feedback (currValue, nextValue)
        else
            return Control.DiscardWith nextValue
    }
    |> Gen.toListn 4
    |> should equal [ 10; 32; 54; 76 ]
    
let [<TestCase>] ``Stop (gen)`` () =
    gen {
        let! v = count 0 1
        if v < 5 then
            return Control.Emit v
        else
            return Control.Stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]

let [<TestCase>] ``Stop (fdb)`` () =
    fdb {
        let! v = Init 0
        let nextValue = v + 1
        if v < 5 then
            return Control.Feedback(v, nextValue)
        else
            return Control.Stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]

////let [<TestCase>] ``Reset (gen)`` () =
////    gen {
////        let! prove = count 0 1
////        let! v = gen {
////            let! c1 = count 0 1
////            let! c2 = count 0 1
////            if c1 = 3 && c2 = 3 then
////                return Control.Reset
////            else
////                return Control.EmitAndLoop c
////        }
////        return Control.EmitAndLoop (prove, v)
////    }
////    |> Gen.toListn 9
////    |> should equal 
////        [
////            0,0; 1,1; 2,2
////            3,0; 4,1; 5,2
////            6,0; 7,1; 8,2
////        ]

let [<TestCase>] ``Singleton`` () =
    gen {
        return Control.Emit 0
        return Control.Stop
    }
    |> Gen.toList
    |> should equal [ 0 ]


let [<TestCase>] ``Combine`` () =
    gen {
        return Control.Emit 0
        return Control.Emit 1
        return Control.Emit 2
        return! Gen.ofList [ 3; 4; 5 ]
        return Control.Emit 6
        return! Gen.ofList [ 7; 8; 9 ]
        return Control.Emit 10
        return Control.Emit 11
    }
    |> Gen.toList
    |> should equal
        [
            0; 1; 2; 3; 6; 7; 10; 11
            0; 1; 2; 4; 6; 8; 10; 11
            0; 1; 2; 5; 6; 9; 10; 11
            0; 1; 2
        ]
