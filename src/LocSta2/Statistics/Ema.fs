module LocSta.Blocks.Statistics.Ema

open LocSta.Core

/// Exponential moving average with smoothing factor 'alpha' (0..1).
let ema alpha =
    stream {
        let! ctx = getCtx()
        let! prev = useStateWith (fun () -> ctx)
        let result = alpha * ctx + (1.0 - alpha) * prev.Value
        prev.Value <- result
        return result
    }
