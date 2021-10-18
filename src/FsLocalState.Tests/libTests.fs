
module LibTests

open FsUnit
open FsLocalState
open FsLocalState.Lib
open NUnit.Framework


let [<TestCase>] ``Reset With`` () =
    let expectedResult = List.replicate 10 [ for i in 0..4 do i ] |> List.collect id

    count 0 1
    |> Gen.resetWith (fun v -> v = 5)
    |> Gen.toListn 50
    |> should equal expectedResult

