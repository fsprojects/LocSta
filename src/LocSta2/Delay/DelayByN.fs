module LocSta.Blocks.Delay.DelayByN

open LocSta.Core

/// Delays the input stream by 'n' samples, filling with 'defaultValue' initially.
let delayByN n defaultValue =
    stream {
        let! ctx = getCtx()
        let! buffer = useStateWith (fun () -> List.replicate n defaultValue)
        let output =
            match buffer.Value with
            | [] -> ctx
            | head :: _ -> head
        if n > 0 then
            buffer.Value <-
                match buffer.Value with
                | [] -> [ ctx ]
                | _ :: tail -> tail @ [ ctx ]
        return output
    }
