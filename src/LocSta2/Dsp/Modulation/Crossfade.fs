module LocSta.Blocks.Dsp.Modulation.Crossfade

open LocSta.Core

/// Crossfades between two streams. mix: 0.0 = s1 only, 1.0 = s2 only.
let crossfade s1 s2 mix =
    stream {
        let! v1 = s1
        let! v2 = s2
        let! m = mix
        return v1 * (1.0 - m) + v2 * m
    }
