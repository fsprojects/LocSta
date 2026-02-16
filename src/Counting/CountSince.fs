module LocSta.Blocks.Counting.CountSince

open LocSta.Core

/// Counts samples since the last true trigger (resets to 0 on trigger).
let countSince () =
    stream {
        let! ctx = getCtx()
        let! count = useState 0
        if ctx then count.Value <- 0
        else count.Value <- count.Value + 1
        return count.Value
    }
