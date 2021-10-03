module FsLocalState.Tests.TestHelper

open FsLocalState

let takeOnceEff eff inputSeq =
    inputSeq |> Gen.toSeqFx eff |> Seq.toList

let takeOnceGen count s =
    let inputSeq = Seq.replicate count ()
    inputSeq |> takeOnceEff (Gen.toEff s)
