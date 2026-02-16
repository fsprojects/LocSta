module LocSta.Blocks.Delay.DelayByN

open LocSta.Core

/// Delays the input stream by 'n' samples, filling with 'defaultValue' initially.
let delayByN n defaultValue =
    stream {
        let! ctx = getCtx()
        let! buffer = useStateWith (fun () -> System.Collections.Generic.Queue<_>(List.replicate n defaultValue))
        let output =
            if buffer.Value.Count = 0 then ctx
            else buffer.Value.Dequeue()
        if n > 0 then buffer.Value.Enqueue ctx
        return output
    }
