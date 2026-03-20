module LocSta.Blocks.Dsp.Oscillators.SineOsc

open LocSta.Core

/// Sine oscillator. Input: frequency (Hz). Requires sampleRate.
let sineOsc sampleRate =
    let invSampleRate = 1.0 / sampleRate
    let twoPi = 2.0 * System.Math.PI
    stream {
        let! freq = getCtx()
        let! phase = useState 0.0
        let value = sin (phase.Value * twoPi)
        phase.Value <- phase.Value + freq * invSampleRate
        if phase.Value >= 1.0 then phase.Value <- phase.Value - 1.0
        return value
    }
