module LocSta.Blocks.Arithmetic.Rescale

open LocSta.Core

/// Linearly maps the input from [inMin, inMax] to [outMin, outMax].
let rescale (inMin: float) inMax outMin outMax =
    let invRange = 1.0 / (inMax - inMin)
    let outRange = outMax - outMin
    stream {
        let! ctx = getCtx()
        return outMin + (ctx - inMin) * invRange * outRange
    }
