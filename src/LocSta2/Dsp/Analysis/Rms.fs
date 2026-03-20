module LocSta.Blocks.Dsp.Analysis.Rms

open LocSta.Core

let private computeRms (arr: float array) (count: int) =
    let mutable sumSq = 0.0
    for i = 0 to count - 1 do
        sumSq <- sumSq + arr[i] * arr[i]
    sqrt (sumSq / float count)

/// RMS (Root Mean Square) level over a window of 'windowSize' samples.
let rms windowSize =
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
        return computeRms arr newCount
    }
