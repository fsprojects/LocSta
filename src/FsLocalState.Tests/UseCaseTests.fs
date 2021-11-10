module UseCaseTests

open FsUnit
open FsLocalState
open NUnit.Framework


let [<TestCase>] ``Stop after Emit`` () =
    let expect = 3
    loop {
        let! c = count 0 1
        if c = expect then
            return Loop.Emit c
            return Loop.Stop
    }
    |> Gen.toList
    |> List.exactlyOne
    |> should equal expect


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
