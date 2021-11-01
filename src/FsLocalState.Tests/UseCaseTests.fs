module UseCaseTests

open FsUnit
open FsLocalState
open NUnit.Framework


//let [<TestCase>] ``Take value and (n - 1) successors`` () =
//    let taken count =
//        fun input -> fdb {
//            let! elements = init []
//            let newElements = input :: elements
//            if newElements.Length = count then
//                return fdb.value newElements newElements
//            else
//                return FdbResult.DiscardWith (Some newElements)
//        }
    
//    let inputValues = [ 1; 3; 6; 3; 0; 0; 4; 5 ]
//    gen {
//        let! values = taken

//    }



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
