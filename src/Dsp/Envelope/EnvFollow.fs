module LocSta.Blocks.Dsp.Envelope.EnvFollow

open LocSta.Core

/// Envelope follower with separate attack and release times. Input: signal. Requires sampleRate.
let envFollow sampleRate attackMs releaseMs =
    stream {
        let! signal = getCtx()
        let! env = useState 0.0
        let absSignal = abs signal
        let attackCoeff = exp (-1.0 / (attackMs * 0.001 * sampleRate))
        let releaseCoeff = exp (-1.0 / (releaseMs * 0.001 * sampleRate))
        let coeff = if absSignal > env.Value then attackCoeff else releaseCoeff
        env.Value <- coeff * env.Value + (1.0 - coeff) * absSignal
        return env.Value
    }
