module LocSta.Blocks.Dsp.Filters.LowPass1

open LocSta.Core

/// 1-pole low-pass filter. Input: (signal, cutoffHz). Requires sampleRate.
let lowPass1 sampleRate =
    stream {
        let! (signal, cutoff) = getCtx()
        let! prev = useStateWith (fun () -> signal)
        let rc = 1.0 / (2.0 * System.Math.PI * cutoff)
        let dt = 1.0 / sampleRate
        let alpha = dt / (rc + dt)
        let output = prev.Value + alpha * (signal - prev.Value)
        prev.Value <- output
        return output
    }
