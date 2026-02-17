module LocSta.Blocks.Dsp.Oscillators.TriangleOsc

open LocSta.Core

/// Triangle oscillator (-1..1). Input: frequency (Hz). Requires sampleRate.
let triangleOsc sampleRate =
    stream {
        let! freq = getCtx()
        let! phase = useState 0.0
        let value = 2.0 * abs (2.0 * phase.Value - 1.0) - 1.0
        phase.Value <- phase.Value + freq / sampleRate
        if phase.Value >= 1.0 then phase.Value <- phase.Value - 1.0
        return value
    }
