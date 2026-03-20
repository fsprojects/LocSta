module LocSta.Blocks.Counting.Rate

open LocSta.Core

let private countTrue (arr: bool array) (count: int) =
    let mutable c = 0
    for i = 0 to count - 1 do
        if arr[i] then c <- c + 1
    c

/// Ratio of true values in a sliding window of 'windowSize' (0.0 to 1.0).
let rate windowSize =
    stream {
        let! ctx = getCtx()
        let! state = useStateWith (fun () ->
            let arr = Array.create windowSize false
            MutableValue(arr, 0, 0))
        let (arr, idx, count) = state.Value.Value
        let newCount = min (count + 1) windowSize
        arr[idx] <- ctx
        let nextIdx = (idx + 1) % windowSize
        state.Value.Value <- (arr, nextIdx, newCount)
        return float (countTrue arr newCount) / float newCount
    }
