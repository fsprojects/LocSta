module LocSta.Blocks.Dsp.Filters.HighPass1

open LocSta.Core

/// 1-pole high-pass filter. Input: (signal, cutoffHz). Requires sampleRate.
let highPass1 sampleRate =
    let dt = 1.0 / sampleRate
    let twoPi = 2.0 * System.Math.PI
    stream {
        let! (signal, cutoff) = getCtx()
        let! prevInput = useStateWith (fun () -> signal)
        let! prevOutput = useState 0.0
        let rc = 1.0 / (twoPi * cutoff)
        let alpha = rc / (rc + dt)
        let output = alpha * (prevOutput.Value + signal - prevInput.Value)
        prevInput.Value <- signal
        prevOutput.Value <- output
        return output
    }
