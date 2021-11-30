#if INTERACTIVE
#r "../LocSta/bin/Debug/netstandard2.0/LocSta.dll"
open LocSta
#endif

module ``Use Case Tests``

open TestHelper
open FsUnit
open LocSta
open LocSta.Lib.Gen
open NUnit.Framework


// TODO: More systematic testing
// TODO: ofListAllAtOnce


let [<TestCase>] ``Pairwise let! (loop)`` () =
    loop {
        let! v1 = Gen.ofListOneByOne [ "a"; "b"; "c"; "d" ]
        let! v2 = Gen.ofListOneByOne [  1 ;  2 ;  3 ;  4  ]
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


let [<TestCase>] ``Combine ofListOneByOne (loop)`` () =
    loop {
        let! v1 = [ "a"; "b"; "c"; "d" ] |> Gen.ofListOneByOne
        yield v1
        let! v2 = [  1 ;  2 ;  3 ;  4  ] |> Gen.ofListOneByOne
        yield v2.ToString()
        yield "X"
    }
    |> Gen.toListn 24
    |> equals 
        [ 
            "a"; "1"; "X"; "b"; "2"; "X"; "c"; "3"; "X"; "d"; "4"; "X"
            (* don't repeat, since Gen.ofListOneByOne stops after the list has ended. *)
        ]


let [<TestCase>] ``Combine + ofListOneByOne + onStopThenReset (loop)`` () =
    loop {
        let! v1 = [ "a"; "b"; "c"; "d" ] |> Gen.ofListOneByOne |> Gen.onStopThenReset
        yield v1
        let! v2 = [  1 ;  2 ;  3 ;  4  ] |> Gen.ofListOneByOne |> Gen.onStopThenReset
        yield v2.ToString()
        yield "X"
    }
    |> Gen.toListn 24
    |> equals 
        [
            "a"; "1"; "X"; "b"; "2"; "X"; "c"; "3"; "X"; "d"; "4"; "X"
             (* repeat due to 'onStopThenReset' *)
            "a"; "1"; "X"; "b"; "2"; "X"; "c"; "3"; "X"; "d"; "4"; "X"
        ]


// TODO
//let [<TestCase>] ``Combine + ofListAllAtOnce + onStopThenReset (loop)`` () =
//    loop {
//        let! v1 = [ "a"; "b"; "c"; "d" ] |> Gen.ofListAllAtOnce |> Gen.onStopThenReset
//        yield v1
//        let! v2 = [  1 ;  2 ;  3 ;  4  ] |> Gen.ofListAllAtOnce |> Gen.onStopThenReset
//        yield v2.ToString()
//        yield "X"
//    }
//    |> Gen.toListn 24
//    |> equals
//        [ 
//            "a"; "b"; "c"; "d"
//            "1"; "X"; "2"; "X"; "3"; "X"; "4"; "X";
//            (* repeat due to 'onStopThenReset' *)
//            "a"; "b"; "c"; "d"
//            "1"; "X"; "2"; "X"; "3"; "X"; "4"; "X";
//        ]
        

let [<TestCase>] ``Combine + for (loop)`` () =
    loop {
        for v1 in [ "a"; "b"; "c"; "d" ] do
            yield v1
        for v2 in [  1 ;  2 ;  3 ;  4  ] do
            yield v2.ToString()
            yield "X"
    }
    |> Gen.toListn 24
    |> equals
        [
            (* 'for' is equivalent to: Gen.ofListAllAtOnce >> Gen.onStopThenReset *)

            "a"; "b"; "c"; "d"
            "1"; "X"; "2"; "X"; "3"; "X"; "4"; "X";
            (* repeat *)
            "a"; "b"; "c"; "d"
            "1"; "X"; "2"; "X"; "3"; "X"; "4"; "X";
        ]


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
            return Feed.Skip nextValue
    }
    |> Gen.toListn 4
    |> equals [ (0 + 0 + 0); (1 + 10 + 3); (2 + 20 + 6); (3 + 30 + 9) ]
    




 //TODO: Document "if" behaviour (also in combination with combine)
let [<TestCase>] ``ResetThis + Combine`` () =
    let n = 3
    feed {
        let! state = Init 1
        let! c = count 10 10
        let vf = state + c, state + 1
        if state = n then
            yield vf
            return Feed.SkipAndResetFeedback
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
    feed {
        let! state = Init 1
        let! c = count 10 10

        let vf = state + c, state + 1
        if state = 4 then
            yield vf
            return Feed.SkipAndResetFeedback
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



//let [<TestCase>] ``Repeating many values in sequence with 'Gen.ofListAllAtOnce'`` () =
//    Gen.ofListOneByOne [5;6]
//    |> Gen.onStopThenReset
//    |> Gen.toListn 10
//    |> equals [ 5; 6; 5; 6; 5; 6; 5; 6; 5; 6 ]





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
//                    return Res.skip (start, distance + 1)
//        }

//        return Res.value 0
//    }
//    |> Gen.toListn 9
//    |> List.last
//    |> equals
//        [
//            [ 8; 7; 6 ]
//            [ 5; 4; 3 ]
//            [ 2; 1; 0 ]
//        ]
