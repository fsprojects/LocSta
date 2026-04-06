module LocSta.Blocks.Detection.Threshold

open LocSta.Core

/// Detects when the input crosses above (1) or below (-1) a fixed threshold level.
let inline threshold level =
    stream {
        let! ctx = getCtx()
        let! st = useMemoWith (fun () -> MutableValue(ctx, false))
        let (p, hasPrev) = st.Value
        let output =
            if not hasPrev then 0
            elif p < level && ctx >= level then 1
            elif p >= level && ctx < level then -1
            else 0
        st.Value <- (ctx, true)
        return output
    }
