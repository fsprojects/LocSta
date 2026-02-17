module LocSta.Blocks.Dsp.Dynamics.HardClip

open LocSta.Core

/// Hard clipper / limiter. Clips signal to [-threshold, +threshold].
let inline hardClip threshold =
    stream {
        let! signal = getCtx()
        return signal |> max (-threshold) |> min threshold
    }
