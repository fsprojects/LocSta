module LocSta.Blocks.Dsp.Filters.HighPass1

open LocSta.Core

/// 1-pole high-pass filter. Input: (signal, cutoffHz). Requires sampleRate.
let highPass1 sampleRate =
    let dt = 1.0 / sampleRate
    let twoPi = 2.0 * System.Math.PI
    stream {
        let! (signal, cutoff) = getCtx()
        let! st = useMemoWith (fun () -> MutableValue(signal, 0.0))
        let (prevInput, prevOutput) = st.Value
        let rc = 1.0 / (twoPi * cutoff)
        let alpha = rc / (rc + dt)
        let output = alpha * (prevOutput + signal - prevInput)
        st.Value <- (signal, output)
        return output
    }
