module FsLocalState.Tests.TestHelper

open FsLocalState.Eval

let takeGenOnce count s =
    (Gen.toEvaluableValues ignore s) count
