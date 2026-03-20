module LocSta.Blocks.Statistics.Ema

open LocSta.Core

/// Exponential moving average with smoothing factor 'alpha' (0..1).
let ema alpha =
    let oneMinusAlpha = 1.0 - alpha
    stream {
        let! ctx = getCtx()
        let! prev = useStateWith (fun () -> ctx)
        let result = alpha * ctx + oneMinusAlpha * prev.Value
        prev.Value <- result
        return result
    }
