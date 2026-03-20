module LocSta.Blocks.Counting.CountIn

open LocSta.Core

let private countTrue (arr: bool array) (count: int) =
    let mutable c = 0
    for i = 0 to count - 1 do
        if arr[i] then c <- c + 1
    c

/// Counts true values within a sliding window of 'windowSize' samples.
let countIn windowSize =
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
        return countTrue arr newCount
    }
