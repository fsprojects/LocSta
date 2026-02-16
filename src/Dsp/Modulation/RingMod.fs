module LocSta.Blocks.Dsp.Modulation.RingMod

open LocSta.Core

/// Ring modulator: multiplies two signals.
let inline ringMod s1 s2 =
    stream {
        let! v1 = s1
        let! v2 = s2
        return v1 * v2
    }
