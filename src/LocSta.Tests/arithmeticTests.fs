#if INTERACTIVE
#r "../LocSta/bin/Debug/netstandard2.0/LocSta.dll"
open LocSta
let equals (expected: 'a) (actual: 'a) = expected = actual
#endif

module LocSta.ArithmeticTests

open TestHelper
open FsUnit
open LocSta
open LocSta.Lib.Gen
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
    Gen.singleton 1 + Gen.singleton 2
    |> Gen.head
    |> equals 3

// TODO: some more
// TODO: test repeat
