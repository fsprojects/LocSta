module LocSta.Blocks.Dsp.Filters.BiquadLowPass

open LocSta.Core

/// Biquad low-pass filter (2nd order, resonant). Input: (signal, cutoffHz, q). Requires sampleRate.
let biquadLowPass sampleRate =
    stream {
        let! (signal, cutoff, q) = getCtx()
        let! x1 = useState 0.0
        let! x2 = useState 0.0
        let! y1 = useState 0.0
        let! y2 = useState 0.0
        let w0 = 2.0 * System.Math.PI * cutoff / sampleRate
        let cosW0 = cos w0
        let sinW0 = sin w0
        let alpha = sinW0 / (2.0 * q)
        let b0 = (1.0 - cosW0) / 2.0
        let b1 = 1.0 - cosW0
        let b2 = (1.0 - cosW0) / 2.0
        let a0 = 1.0 + alpha
        let a1 = -2.0 * cosW0
        let a2 = 1.0 - alpha
        let output = (b0 / a0) * signal + (b1 / a0) * x1.Value + (b2 / a0) * x2.Value
                     - (a1 / a0) * y1.Value - (a2 / a0) * y2.Value
        x2.Value <- x1.Value
        x1.Value <- signal
        y2.Value <- y1.Value
        y1.Value <- output
        return output
    }
