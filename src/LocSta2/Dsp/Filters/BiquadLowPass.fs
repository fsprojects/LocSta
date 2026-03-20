module LocSta.Blocks.Dsp.Filters.BiquadLowPass

open LocSta.Core

/// Biquad low-pass filter (2nd order, resonant). Input: (signal, cutoffHz, q). Requires sampleRate.
let biquadLowPass sampleRate =
    let twoPiOverSr = 2.0 * System.Math.PI / sampleRate
    stream {
        let! (signal, cutoff, q) = getCtx()
        let! st = useMemoWith (fun () -> MutableValue(0.0, 0.0, 0.0, 0.0))
        let (x1, x2, y1, y2) = st.Value
        let w0 = twoPiOverSr * cutoff
        let cosW0 = cos w0
        let alpha = sin w0 / (2.0 * q)
        let a0inv = 1.0 / (1.0 + alpha)
        let halfOneMinusCos = (1.0 - cosW0) * 0.5
        let output = (halfOneMinusCos * signal + (1.0 - cosW0) * x1 + halfOneMinusCos * x2
                     + 2.0 * cosW0 * y1 - (1.0 - alpha) * y2) * a0inv
        st.Value <- (signal, x1, output, y1)
        return output
    }
