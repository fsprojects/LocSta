module LocSta.Blocks.Dsp.Filters.BiquadHighPass

open LocSta.Core

/// Biquad high-pass filter (2nd order, resonant). Input: (signal, cutoffHz, q). Requires sampleRate.
let biquadHighPass sampleRate =
    stream {
        let! (signal, cutoff, q) = getCtx()
        let! x1 = useState 0.0
        let! x2 = useState 0.0
        let! y1 = useState 0.0
        let! y2 = useState 0.0
        let w0 = 2.0 * System.Math.PI * cutoff / sampleRate
        let cosW0 = cos w0
        let alpha = sin w0 / (2.0 * q)
        let a0inv = 1.0 / (1.0 + alpha)
        let halfOnePlusCos = (1.0 + cosW0) * 0.5
        let output = (halfOnePlusCos * signal - (1.0 + cosW0) * x1.Value + halfOnePlusCos * x2.Value
                     + 2.0 * cosW0 * y1.Value - (1.0 - alpha) * y2.Value) * a0inv
        x2.Value <- x1.Value
        x1.Value <- signal
        y2.Value <- y1.Value
        y1.Value <- output
        return output
    }
