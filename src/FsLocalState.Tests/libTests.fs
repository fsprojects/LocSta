#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
#endif

module LibTests

open FsUnit
open FsLocalState
open NUnit.Framework


let [<TestCase>] ``Reset by current`` () =
    let expectedResult = List.replicate 10 [ for i in 0..4 do i ] |> List.collect id

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

let [<TestCase>] ``Filter map`` () =
    // Task:
    // All value that have a successor
    //   * which is >= 2 times the value
    //   * in a maximum distance of 4 values
    [ 1; 2; 3; 4; 4; 0; 0; 10 ; 20 ]
    |> Gen.ofList
    |> Gen.filterMapFx (fun i -> 0 => fun count -> gen {
        if count > 4 then
            yield S
        else
            return "Super", count + 1
    })
    |> Gen.toListn 9
    |> List.last
    |> should equal
        [
            [ 8; 7; 6 ]
            [ 5; 4; 3 ]
            [ 2; 1; 0 ]
        ]
