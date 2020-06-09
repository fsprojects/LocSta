module FsLocalState.Tests.TestHelper

open FsLocalState

let pullN n =
    Seq.take n
    >> Seq.toList

let toValuesN sampleCount =
    Eval.toSeqGen ignore
    >> Seq.map Eval.getValue
    >> pullN sampleCount
