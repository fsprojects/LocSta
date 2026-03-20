module LocSta.Blocks.Dsp.Analysis.ZeroCrossRate

open LocSta.Core

let private computeZeroCrossings (arr: float array) (count: int) (startIdx: int) (windowSize: int) =
    if count < 2 then 0.0
    else
        let mutable crossings = 0
        for k = 0 to count - 2 do
            let i = (startIdx + k) % windowSize
            let j = (startIdx + k + 1) % windowSize
            if (arr[i] >= 0.0 && arr[j] < 0.0) || (arr[i] < 0.0 && arr[j] >= 0.0) then
                crossings <- crossings + 1
        float crossings / float (count - 1)

/// Zero crossing rate over a window of 'windowSize' samples.
let zeroCrossRate windowSize =
    stream {
        let! signal = getCtx()
        let! state = useStateWith (fun () ->
            let arr = Array.zeroCreate<float> windowSize
            MutableValue(arr, 0, 0))
        let (arr, idx, count) = state.Value.Value
        let newCount = min (count + 1) windowSize
        arr[idx] <- signal
        let nextIdx = (idx + 1) % windowSize
        state.Value.Value <- (arr, nextIdx, newCount)
        let startIdx = if newCount < windowSize then 0 else nextIdx
        return computeZeroCrossings arr newCount startIdx windowSize
    }
