module LocSta.Blocks.Dsp.Oscillators.SineOsc

open LocSta.Core

/// Sine oscillator. Input: frequency (Hz). Requires sampleRate.
let sineOsc sampleRate =
    stream {
        let! freq = getCtx()
        let! phase = useState 0.0
        let value = sin (phase.Value * 2.0 * System.Math.PI)
        phase.Value <- phase.Value + freq / sampleRate
        if phase.Value >= 1.0 then phase.Value <- phase.Value - 1.0
        return value
    }
