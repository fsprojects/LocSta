#if INTERACTIVE
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module ArithmeticTests

open FsUnit
open FsLocalState
open FsLocalState.Lib.Gen
open NUnit.Framework

let [<TestCase>] ``Operator == gen int`` () =
    loop {
        let! prove = count 0 1
        let! res = count 0 1 == 5
        if res then
            yield prove
        else if prove > 10 then
            return Loop.Stop
    }
    |> Gen.toListn 10
    |> should equal [ 5 ]

let [<TestCase>] ``Operator + gen gen`` () =
    Gen.returnValueOnce 1 + Gen.returnValueOnce 2
    |> Gen.head
    |> should equal 3

// TODO: some more
