module UseCaseTests

open FsUnit
open FsLocalState
open NUnit.Framework


let [<TestCase>] ``Stop after Emit`` () =
    let expect = 3
    gen {
        let! c = count01
        if c = expect then
            return Control.Emit c
            return Control.Stop
    }
    |> Gen.toList
    |> List.exactlyOne
    |> should equal expect


//let [<TestCase>] ``Accumulate value and 2 successors`` () =
//    let inputValues = [ 1; 3; 6; 3; 0; 0; 4; 5 ]

//    let taken num =
//        fun input -> fdb {
//            let! elements = init []

//        }

//    fdb {
//        let! state = init []
//        for v in inputValues do
        
//    }
//    |> Gen.toListn 9
//    |> List.last
//    |> should equal
//        [
//            [ 8; 7; 6 ]
//            [ 5; 4; 3 ]
//            [ 2; 1; 0 ]
//        ]
