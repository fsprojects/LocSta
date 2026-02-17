module LocSta.Blocks.Dsp.Oscillators.SquareOsc

open LocSta.Core

/// Square oscillator with pulse width (0..1). Input: (frequency, pulseWidth). Requires sampleRate.
let squareOsc sampleRate =
    stream {
        let! (freq, pulseWidth) = getCtx()
        let! phase = useState 0.0
        let value = if phase.Value < pulseWidth then 1.0 else -1.0
        phase.Value <- phase.Value + freq / sampleRate
        if phase.Value >= 1.0 then phase.Value <- phase.Value - 1.0
        return value
    }
