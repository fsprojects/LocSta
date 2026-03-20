module LocSta.Blocks.Windowing.WindowedReduce

open LocSta.Core

let foldWindow (arr: 'a array) (count: int) (startIdx: int) (windowSize: int) folder seed =
    let mutable acc = seed
    for k = 0 to count - 1 do
        let i = (startIdx + k) % windowSize
        acc <- folder acc arr[i]
    acc

/// Applies a fold over a sliding window of 'windowSize' samples.
let inline windowedReduce windowSize ([<InlineIfLambda>] folder) seed =
    stream {
        let! ctx = getCtx()
        let! state = useStateWith (fun () ->
            MutableValue(Array.zeroCreate windowSize, 0, 0))
        let (arr, idx, count) = state.Value.Value
        let newCount = min (count + 1) windowSize
        arr[idx] <- ctx
        let nextIdx = (idx + 1) % windowSize
        state.Value.Value <- (arr, nextIdx, newCount)
        let startIdx = if newCount < windowSize then 0 else nextIdx
        return foldWindow arr newCount startIdx windowSize folder seed
    }
