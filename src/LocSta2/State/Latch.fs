module LocSta.Blocks.State.Latch

open LocSta.Core

/// Captures the value when trigger is true; holds it until the next trigger.
let latch defaultValue =
    stream {
        let! (trigger, value) = getCtx()
        let! held = useState defaultValue
        if trigger then held.Value <- value
        return held.Value
    }
