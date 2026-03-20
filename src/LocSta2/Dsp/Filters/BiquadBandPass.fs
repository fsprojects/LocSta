module LocSta.Blocks.Dsp.Filters.BiquadBandPass

open LocSta.Core

/// Biquad band-pass filter (2nd order). Input: (signal, centerHz, q). Requires sampleRate.
let biquadBandPass sampleRate =
    let twoPiOverSr = 2.0 * System.Math.PI / sampleRate
    stream {
        let! (signal, center, q) = getCtx()
        let! st = useMemoWith (fun () -> MutableValue(0.0, 0.0, 0.0, 0.0))
        let (x1, x2, y1, y2) = st.Value
        let w0 = twoPiOverSr * center
        let sinW0 = sin w0
        let cosW0 = cos w0
        let alpha = sinW0 / (2.0 * q)
        let a0inv = 1.0 / (1.0 + alpha)
        let output = (alpha * signal - alpha * x2
                     + 2.0 * cosW0 * y1 - (1.0 - alpha) * y2) * a0inv
        st.Value <- (signal, x1, output, y1)
        return output
    }
