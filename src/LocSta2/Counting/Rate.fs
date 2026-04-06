module LocSta.Blocks.Counting.Rate

open LocSta.Core

/// Ratio of true values in a sliding window of 'windowSize' (0.0 to 1.0).
let rate windowSize =
    stream {
        let! ctx = getCtx()
        let! state = useStateWith (fun () ->
            let arr = Array.create windowSize false
            MutableValue(arr, 0, 0, 0))
        let (arr, idx, count, trueCount) = state.Value.Value
        let newCount = min (count + 1) windowSize
        // Subtract old value being overwritten (only if buffer is full)
        let removed = if count >= windowSize && arr[idx] then 1 else 0
        let added = if ctx then 1 else 0
        let newTrueCount = trueCount - removed + added
        arr[idx] <- ctx
        let nextIdx = (idx + 1) % windowSize
        state.Value.Value <- (arr, nextIdx, newCount, newTrueCount)
        return float newTrueCount / float newCount
    }
