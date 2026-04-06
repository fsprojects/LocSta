module LocSta.Blocks.Statistics.MovingAverage

open LocSta.Core

/// Simple moving average over the last 'windowSize' samples.
let movingAverage windowSize =
    stream {
        let! ctx = getCtx()
        let! state = useStateWith (fun () ->
            let arr = Array.zeroCreate<int> windowSize
            MutableValue(arr, 0, 0, 0))
        let (arr, idx, count, runningSum) = state.Value.Value
        let newCount = min (count + 1) windowSize
        let oldVal = if count >= windowSize then arr[idx] else 0
        let newSum = runningSum - oldVal + ctx
        arr[idx] <- ctx
        let nextIdx = (idx + 1) % windowSize
        state.Value.Value <- (arr, nextIdx, newCount, newSum)
        return (float newSum) / (float newCount)
    }
