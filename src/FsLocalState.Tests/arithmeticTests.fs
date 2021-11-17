#if INTERACTIVE
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
let equals (expected: 'a) (actual: 'a) = expected = actual
#endif

module FsLocalState.ArithmeticTests

open TestHelper
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
    |> equals [ 5 ]

let [<TestCase>] ``Operator + gen gen`` () =
    Gen.ofValue 1 + Gen.ofValue 2
    |> Gen.head
    |> equals 3

// TODO: some more
