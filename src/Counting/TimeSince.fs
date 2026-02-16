module LocSta.Blocks.Counting.TimeSince

open LocSta.Core

/// Measures elapsed time since the last trigger. Input: (trigger, time).
let inline timeSince () =
    stream {
        let! (trigger, time) = getCtx()
        let! lastTime = useStateWith (fun () -> time)
        if trigger then lastTime.Value <- time
        return time - lastTime.Value
    }
