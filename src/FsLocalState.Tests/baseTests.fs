
module BaseTests

open FsUnit
open FsLocalState
open NUnit.Framework


let [<TestCase>] ``Pairwise`` () =
    gen {
        let! v1 = Gen.ofList [ "a"; "b"; "c"; "d" ]
        let! v2 = Gen.ofList [  1 ;  2 ;  3 ;  4  ]
        return gen.value (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Pairwise using For Loop`` () =
    gen {
        for v1 in [ "a"; "b"; "c"; "d" ] do
        for v2 in [  1 ;  2 ;  3 ;  4  ] do
            return gen.value (v1,v2)
    }
    |> Gen.toList
    |> should equal [ ("a", 1); ("b", 2); ("c", 3); ("d", 4) ]


let [<TestCase>] ``Discard values (gen)`` () =
    gen {
        let! v = [ 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            return gen.value v
    }
    |> Gen.toList
    |> should equal [ 2; 4; 6 ]


let [<TestCase>] ``Discard value using For Loop (gen)`` () =
    gen {
        for v in [ 1; 2; 3; 4; 5; 6 ] do
            if v % 2 = 0 then
                return gen.value v
    }
    |> Gen.toList
    |> should equal [ 2; 4; 6 ]

    
let [<TestCase>] ``Discard values (fdb)`` () =
    fdb {
        let! state = init 0
        if state % 2 = 0 then
            return fdb.value state (state + 1)
    }
    |> Gen.toList
    |> should equal [ 2; 4; 6 ]
    