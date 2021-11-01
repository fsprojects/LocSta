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
        return Res.value (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Pairwise using For Loop`` () =
    gen {
        for v1 in [ "a"; "b"; "c"; "d" ] do
        for v2 in [  1 ;  2 ;  3 ;  4  ] do
            return Res.value (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Discard values (gen)`` () =
    gen {
        let! v = [ 0; 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            return Res.value v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]


let [<TestCase>] ``Discard value using For Loop (gen)`` () =
    gen {
        for v in [ 0; 1; 2; 3; 4; 5; 6 ] do
            if v % 2 = 0 then
                return Res.value v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]

    
let [<TestCase>] ``Discard values (fdb)`` () =
    fdb {
        let! state = Init 0
        let nextValue = state + 1
        if state % 2 = 0 then
            return Res.feedback state nextValue
        else
            return Res.discardWith nextValue
    }
    |> Gen.toListn 4
    |> should equal [ 0; 2; 4; 6 ]
    

let [<TestCase>] ``Stop (gen)`` () =
    gen {
        let! v = count01
        if v < 5 then
            return Res.value v
        else
            return Res.stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]


let [<TestCase>] ``Stop (fdb)`` () =
    fdb {
        let! v = Init 0
        let nextValue = v + 1
        if v < 5 then
            return Res.feedback v nextValue
        else
            return Res.stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]


let [<TestCase>] ``Singleton`` () =
    gen {
        return Res.valueAndStop 0
    }
    |> Gen.toList
    |> should equal [ 0 ]


let [<TestCase>] ``Combine`` () =
    gen {
        return Res.valueAndStop 0
        return Res.valueAndStop 1
        return! Gen.ofList [ 2; 3; 4 ]
    }
    |> Gen.toList
    |> should equal [ 0; 1; 2; 3; 4 ]
