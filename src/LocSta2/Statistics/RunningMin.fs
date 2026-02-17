module LocSta.Blocks.Statistics.RunningMin

open LocSta.Core

/// All-time minimum: tracks the lowest value ever seen.
let inline runningMin () =
    stream {
        let! ctx = getCtx()
        let! best = useStateWith (fun () -> ctx)
        if ctx < best.Value then best.Value <- ctx
        return best.Value
    }
