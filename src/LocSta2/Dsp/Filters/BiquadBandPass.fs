module LocSta.Blocks.Dsp.Filters.BiquadBandPass

open LocSta.Core

/// Biquad band-pass filter (2nd order). Input: (signal, centerHz, q). Requires sampleRate.
let biquadBandPass sampleRate =
    let twoPiOverSr = 2.0 * System.Math.PI / sampleRate
    stream {
        let! (signal, center, q) = getCtx()
        let! x1 = useState 0.0
        let! x2 = useState 0.0
        let! y1 = useState 0.0
        let! y2 = useState 0.0
        let w0 = twoPiOverSr * center
        let sinW0 = sin w0
        let cosW0 = cos w0
        let alpha = sinW0 / (2.0 * q)
        let a0inv = 1.0 / (1.0 + alpha)
        let output = (alpha * signal - alpha * x2.Value
                     + 2.0 * cosW0 * y1.Value - (1.0 - alpha) * y2.Value) * a0inv
        x2.Value <- x1.Value
        x1.Value <- signal
        y2.Value <- y1.Value
        y1.Value <- output
        return output
    }
