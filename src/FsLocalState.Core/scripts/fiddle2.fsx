
#r "../bin/Debug/netstandard2.0/FsLocalState.dll"

open FsLocalState
open FsLocalState.Core

type Env =
    { samplePos: int
      sampleRate: int }

let toSeconds env = (double env.samplePos) / (double env.sampleRate)


/// Converts a signal and a given sample rate to a sequence.
let toAudioSeq (local: Local<_, _, Env>) sampleRate =
    local
    |> Eval.toSeqGen (fun i ->
        { samplePos = i
          sampleRate = sampleRate })

/// Converts a signal with a sample rate of 44.1kHz to a sequence.
let toAudioSeq44k (local: Local<_, _, _>) = toAudioSeq local 44100

module Test =

    let evalN sr local =
        toAudioSeq local sr |> listN

    let evalN44k local =
        toAudioSeq44k local |> listN

