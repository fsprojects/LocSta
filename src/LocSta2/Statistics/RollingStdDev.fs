module LocSta.Blocks.Statistics.RollingStdDev

open LocSta.Core

let private computeStdDev (arr: float array) (count: int) =
    let n = float count
    let mutable sum = 0.0
    for i = 0 to count - 1 do
        sum <- sum + arr[i]
    let mean = sum / n
    let mutable variance = 0.0
    for i = 0 to count - 1 do
        let d = arr[i] - mean
        variance <- variance + d * d
    sqrt (variance / n)

/// Rolling standard deviation over the last 'windowSize' samples.
let rollingStdDev windowSize =
    stream {
        let! ctx = getCtx()
        let! state = useStateWith (fun () ->
            let arr = Array.zeroCreate<float> windowSize
            MutableValue(arr, 0, 0))
        let (arr, idx, count) = state.Value.Value
        let newCount = min (count + 1) windowSize
        arr[idx] <- ctx
        let nextIdx = (idx + 1) % windowSize
        state.Value.Value <- (arr, nextIdx, newCount)
        return computeStdDev arr newCount
    }
