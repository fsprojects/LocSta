module LocSta.Blocks.Dsp.Dynamics.SoftClip

open LocSta.Core

/// Soft clipping via tanh saturation. drive controls the amount of distortion (1.0 = mild).
let softClip drive =
    stream {
        let! signal = getCtx()
        return tanh (signal * drive)
    }
