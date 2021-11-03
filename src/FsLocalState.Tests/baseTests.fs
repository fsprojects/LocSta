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
        return Res.EmitAndLoop (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Pairwise using For Loop`` () =
    gen {
        for v1 in [ "a"; "b"; "c"; "d" ] do
        for v2 in [  1 ;  2 ;  3 ;  4  ] do
            return Res.EmitAndLoop (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Discard values (gen)`` () =
    gen {
        let! v = [ 0; 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            return Res.EmitAndLoop v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]


let [<TestCase>] ``Discard value using For Loop (gen)`` () =
    gen {
        for v in [ 0; 1; 2; 3; 4; 5; 6 ] do
            if v % 2 = 0 then
                return Res.EmitAndLoop v
    }
    |> Gen.toList
    |> should equal [ 0; 2; 4; 6 ]

    
let [<TestCase>] ``Discard values (fdb)`` () =
    fdb {
        let! state = Init 0
        let nextValue = state + 1
        if state % 2 = 0 then
            return Res.Feedback (state, nextValue)
        else
            return Res.DiscardWith nextValue
    }
    |> Gen.toListn 4
    |> should equal [ 0; 2; 4; 6 ]
    

let [<TestCase>] ``Stop (gen)`` () =
    gen {
        let! v = count01
        if v < 5 then
            return Res.EmitAndLoop v
        else
            return Res.Stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]


let [<TestCase>] ``Stop (fdb)`` () =
    fdb {
        let! v = Init 0
        let nextValue = v + 1
        if v < 5 then
            return Res.Feedback(v, nextValue)
        else
            return Res.Stop
    }
    |> Gen.toList
    |> should equal [ 0 .. 4 ]


let [<TestCase>] ``Singleton`` () =
    gen {
        return Res.EmitAndStop 0
    }
    |> Gen.toList
    |> should equal [ 0 ]


let [<TestCase>] ``Singleton with Yield`` () =
    gen {
        return Res.EmitAndStop 0
    }
    |> Gen.toList
    |> should equal [ 0 ]


let [<TestCase>] ``Combine`` () =
    gen {
        return Res.EmitAndStop 0
        return Res.EmitAndStop 1
        return! Gen.ofList [ 2; 3; 4 ]
    }
    |> Gen.toList
    |> should equal [ 0; 1; 2; 3; 4 ]

let [<TestCase>] ``Combine with Yield`` () =
    gen {
        return Res.EmitAndStop 0
        return Res.EmitAndStop 1
        yield! [ 2; 3; 4 ]
    }
    |> Gen.toList
    |> should equal [ 0; 1; 2; 3; 4 ]

let [<TestCase>] ``Operator +`` () =
    gen {
        let! res = count01 + count01
        return Res.EmitAndLoop res
    }
    |> Gen.toListn 5
    |> should equal [ 0; 2; 4; 6; 8 ]

let [<TestCase>] ``Operator =`` () =
    gen {
        let! proove = count01
        let! res = count01 == 5
        if res then
            return Res.EmitAndLoop proove
        else if proove > 10 then
            return Res.Stop
        else
            return Res.Discard
    }
    |> Gen.toListn 10
    |> should equal [ 5 ]

