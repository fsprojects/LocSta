module LocSta.Blocks.Arithmetic.Rescale

open LocSta.Core

/// Linearly maps the input from [inMin, inMax] to [outMin, outMax].
let rescale (inMin: float) inMax outMin outMax =
    stream {
        let! ctx = getCtx()
        let normalized = (ctx - inMin) / (inMax - inMin)
        return outMin + normalized * (outMax - outMin)
    }
