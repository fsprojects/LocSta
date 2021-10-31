#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module LibTests

open FsUnit
open FsLocalState
open NUnit.Framework



let [<TestCase>] ``Reset by current`` () =
    let expectedResult =
        [ 0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4
          0; 1; 2; 3; 4 ]

    Gen.count 0 1
    |> Gen.resetWithCurrent (fun v -> v = 5)
    |> Gen.toListn 50
    |> should equal expectedResult


let [<TestCase>] ``Partition by current`` () =
    Gen.count 0 1
    |> Gen.partitionWithCurrent (fun v -> v % 3 = 0)
    |> Gen.toListn 9
    |> List.last
    |> should equal
        [
            [ 8; 7; 6 ]
            [ 5; 4; 3 ]
            [ 2; 1; 0 ]
        ]

let [<TestCase>] ``Example: Partition by current`` () =
    Gen.count 0 1
    |> Gen.partitionWithCurrent (fun v -> v % 3 = 0)
    |> Gen.toListn 9
    |> List.last
    |> should equal
        [
            [ 8; 7; 6 ]
            [ 5; 4; 3 ]
            [ 2; 1; 0 ]
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


//    //[ 1; 2; 3; 4; 4; 0; 0; 10 ; 20 ]
//    //|> Gen.ofList
//    //|> Gen.filterMapFx (fun start curr -> 0 => fun count -> gen {
//    //    printfn $"initial = {start}  |  curr = {curr}"
//    //    if count > 4 then
//    //        printfn "  - BREAK"
//    //        return Res.stop
//    //    else if curr >= start * 2 then
//    //        printfn "  - RET"
//    //        return Res.feedback curr (count + 1)
//    //    else
//    //        printfn $"  - CONT {curr}"
//    //        return Res.discard
//    //})
//    //|> Gen.toListn 9
//    //|> List.last
//    //|> should equal
//    //    [
//    //        [ 8; 7; 6 ]
//    //        [ 5; 4; 3 ]
//    //        [ 2; 1; 0 ]
//    //    ]
