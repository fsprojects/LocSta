module LocSta.Blocks.Detection.Threshold

open LocSta.Core

/// Detects when the input crosses above (1) or below (-1) a fixed threshold level.
let inline threshold level =
    stream {
        let! ctx = getCtx()
        let! prev = useState ValueNone
        let output =
            match prev.Value with
            | ValueNone -> 0
            | ValueSome p ->
                if p < level && ctx >= level then 1
                elif p >= level && ctx < level then -1
                else 0
        prev.Value <- ValueSome ctx
        return output
    }
