#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
#endif

module LibTests

open FsUnit
open FsLocalState
open NUnit.Framework


let [<TestCase>] ``Reset by current`` () =
    let expectedResult = List.replicate 10 [ for i in 0..4 do i ] |> List.collect id

    count 0 1
    |> resetWithCurrent (fun v -> v = 5)
    |> Gen.toListn 50
    |> should equal expectedResult


let [<TestCase>] ``Partition by current`` () =
    count 0 1
    |> partitionWithCurrent (fun v -> v % 3 = 0)
    |> Gen.toListn 9
    |> List.last
    |> should equal
        [
            [ 8; 7; 6 ]
            [ 5; 4; 3 ]
            [ 2; 1; 0 ]
        ]
