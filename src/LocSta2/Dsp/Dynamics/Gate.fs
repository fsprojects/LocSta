module LocSta.Blocks.Dsp.Dynamics.Gate

open LocSta.Core

/// Noise gate. Signal below threshold is silenced. Input: (signal, envelope).
let gate threshold =
    stream {
        let! (signal, envelope) = getCtx()
        return if envelope >= threshold then signal else 0.0
    }
