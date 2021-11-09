#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module ArithmeticTests

open FsUnit
open FsLocalState
open NUnit.Framework

let take1 g = g |> Gen.toListn 1 |> List.exactlyOne


let [<TestCase>] ``Operator == gen int`` () =
    gen {
        let! prove = count 0 1
        let! res = count 0 1 == 5
        if res then
            return Control.Emit prove
        else if prove > 10 then
            return Control.Stop
    }
    |> Gen.toListn 10
    |> should equal [ 5 ]

let [<TestCase>] ``Operator + gen gen`` () =
    Gen.returnValueOnce 1 + Gen.returnValueOnce 2
    |> take1
    |> should equal 3

// TODO: some more
