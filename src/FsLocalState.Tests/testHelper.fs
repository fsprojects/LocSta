module FsLocalState.Tests.TestHelper

open FsLocalState
open FsLocalState.Eval

let takeOnceEff inputSeq s =
    (Eval.Eff.toEvaluableV ignore s) inputSeq

let takeOnceGen count s =
    let inputSeq = Seq.replicate count ()
    takeOnceEff inputSeq (Gen.toEff s)
   
