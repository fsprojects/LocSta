module LocSta.Blocks.Delay.DelayByN

open LocSta.Core

/// Delays the input stream by 'n' samples, filling with 'defaultValue' initially.
let delayByN n defaultValue =
    stream {
        let! ctx = getCtx()
        let! state = useStateWith (fun () ->
            let arr = Array.create (max 1 n) defaultValue
            MutableValue(arr, 0))
        let (arr, idx) = state.Value.Value
        let output =
            if n > 0 then arr[idx]
            else ctx
        arr[idx] <- ctx
        let nextIdx = (idx + 1) % (max 1 n)
        state.Value.Value <- (arr, nextIdx)
        return output
    }
