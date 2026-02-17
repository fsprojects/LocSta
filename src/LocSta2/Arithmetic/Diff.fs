module LocSta.Blocks.Arithmetic.Diff

open LocSta.Core

/// Computes the difference between the current and the previous value.
let inline diff defaultValue =
    stream {
        let! ctx = getCtx()
        let! prev = useState defaultValue
        let output = ctx - prev.Value
        prev.Value <- ctx
        return output
    }
