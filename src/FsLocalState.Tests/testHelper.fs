module FsLocalState.Tests.TestHelper

open FsLocalState

let takeOnceEff inputSeq s =
    (Eff.toSeq ignore s) inputSeq |> Seq.toList

let takeOnceGen count s =
    let inputSeq = Seq.replicate count ()
    takeOnceEff inputSeq (Gen.toEff s)
   
