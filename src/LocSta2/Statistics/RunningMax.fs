module LocSta.Blocks.Statistics.RunningMax

open LocSta.Core

/// All-time maximum: tracks the highest value ever seen.
let inline runningMax () =
    stream {
        let! ctx = getCtx()
        let! best = useStateWith (fun () -> ctx)
        if ctx > best.Value then best.Value <- ctx
        return best.Value
    }
