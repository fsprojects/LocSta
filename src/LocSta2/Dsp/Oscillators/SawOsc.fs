module LocSta.Blocks.Dsp.Oscillators.SawOsc

open LocSta.Core

/// Sawtooth oscillator (-1..1). Input: frequency (Hz). Requires sampleRate.
let sawOsc sampleRate =
    let invSampleRate = 1.0 / sampleRate
    stream {
        let! freq = getCtx()
        let! phase = useState 0.0
        let value = 2.0 * phase.Value - 1.0
        phase.Value <- phase.Value + freq * invSampleRate
        if phase.Value >= 1.0 then phase.Value <- phase.Value - 1.0
        return value
    }
