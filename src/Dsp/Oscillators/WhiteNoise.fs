module LocSta.Blocks.Dsp.Oscillators.WhiteNoise

open LocSta.Core

/// White noise generator (-1..1). Uses seeded random for reproducibility.
let whiteNoise seed =
    stream {
        let! _ = getCtx()
        let! rng = useMemoWith (fun () -> System.Random(seed))
        return rng.NextDouble() * 2.0 - 1.0
    }
