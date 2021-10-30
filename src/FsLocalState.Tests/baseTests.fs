
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


let [<TestCase>] ``Discard Values`` () =
    gen {
        let! v = [ 1; 2; 3; 4; 5; 6 ] |> Gen.ofList
        if v % 2 = 0 then
            return Res.value v
    }
    |> Gen.toList
    |> should equal [ 2; 4; 6 ]


let [<TestCase>] ``For Loop`` () =
    gen {
        for v in [ 1; 2; 3; 4; 5; 6 ] do
            if v % 2 = 0 then
                return Res.value v
    }
    |> Gen.toList
    |> should equal [ 2; 4; 6 ]

