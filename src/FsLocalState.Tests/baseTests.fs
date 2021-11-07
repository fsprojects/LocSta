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

let x =
    Init 0 |> Gen.bindFdb (fun state ->
        Gen.returnFeedback state 12
    )

let y =
    Init 0 |> Gen.bindFdb (fun state ->
        let res =
            count 0 1 |> Gen.bind (fun c ->
                Gen.returnFeedback state (c + 1)
            )
        failwith ""
    )

let k = 
    delay1 2 |> Gen.bind (fun c ->
        Gen.returnValue c
    )

let z =
    Init 0 |> Gen.bindFdb (fun state ->
        let res =
            delay1 2 |> Gen.bind (fun c ->
                Gen.returnValue c
            )
        res
    )
    
let [<TestCase>] ``Discard with (fdb)`` () =
    fdb {
        let! state = Init 0
        let nextValue = state + 1
        if state % 2 = 0 then
            return Control.Feedback (state, nextValue)
        else
            return Control.DiscardWith nextValue
    }
    |> Gen.toListn 4
    |> should equal [ 0; 2; 4; 6 ]
    
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
////        let! prove = count01
////        let! v = gen {
////            let! c1 = count01
////            let! c2 = count01
////            if c1 = 3 $& c2 = 3 then
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
        //return Control.Stop
        return Control.Emit 11
    }
    |> Gen.toList
    |> should equal
        [
            0; 1; 2; 3; 6; 7; 10; 11
            0; 1; 2; 4; 6; 8; 10; 11
            0; 1; 2; 5; 6; 9; 10; 11
        ]
