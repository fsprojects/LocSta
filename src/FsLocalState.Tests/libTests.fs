#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module LibTests

open FsUnit
open FsLocalState
open NUnit.Framework


let [<TestCase>] ``Reset by current`` () =
    Gen.count 0 1
    |> Gen.resetWhenFunc (fun v -> v = 5)
    |> Gen.toListn (5 * 3)
    |> should equal
        [ 0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4 ]

let [<TestCase>] ``Reset on stop`` () =
    [0..2]
    |> Gen.ofList
    |> resetOnStop
    |> Gen.toListn 9
    |> should equal
        [ 0; 1; 2 
          0; 1; 2 
          0; 1; 2 ]

let [<TestCase>] ``Count 0 1`` () =
    Gen.count01
    |> Gen.toListn 5
    |> should equal [0..4]

let [<TestCase>] ``Count until repeat`` () =
    Gen.countCyclic 0 1 3
    |> Gen.toListn 12
    |> should equal
        [
            yield! [0..3]
            yield! [0..3]
            yield! [0..3]
        ]

let [<TestCase>] ``Accumulate onc part`` () =
    gen {
        for x in [0..10] do
            let! values = accumulateOnePart 3 x
            return Control.EmitAndLoop values
    }
    |> Gen.toList
    |> should equal
        [
            [ 2 ; 1 ; 0 ]
        ]

let [<TestCase>] ``Accumulate many parts`` () =
    gen {
        for x in [0..10] do
            let! values = accumulateManyParts 3 x
            return Control.EmitAndLoop values
    }
    |> Gen.toList
    |> should equal
        [
            [ 2 ; 1 ; 0 ]
            [ 5 ; 4 ; 3 ]
            [ 8 ; 7 ; 6 ]
        ]


//let [<TestCase>] ``Filter map`` () =
//    // Task:
//    // All value that have a successor
//    //   * which is >= 2 times the value
//    //   * in a maximum distance of 4 values
//    gen {
//        for v in [ 1; 2; 3; 4; 4; 0; 0; 10 ; 20 ] do
//        let! res = (v,0) => fun (start, distance) -> gen {
//            if distance > 4 then
//                return Res.stop
//            else
//                if v * 2 >= start then
//                    return Res.feedback (start, v) (start, distance + 1)
//                else
//                    return Res.discardWith (start, distance + 1)
//        }

//        return Res.value 0
//    }
//    |> Gen.toListn 9
//    |> List.last
//    |> should equal
//        [
//            [ 8; 7; 6 ]
//            [ 5; 4; 3 ]
//            [ 2; 1; 0 ]
//        ]
