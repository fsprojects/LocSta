module FsLocalState.Tests.TestHelper

open FsLocalState
open FsLocalState.Eval

let takeOnceEff inputSeq s =
    (Eval.Eff.toSeq ignore s) inputSeq |> Seq.toList

let takeOnceGen count s =
    let inputSeq = Seq.replicate count ()
    takeOnceEff inputSeq (Gen.toEff s)
   
