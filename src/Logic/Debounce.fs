module LocSta.Blocks.Logic.Debounce

open LocSta.Core

/// Returns true only after 'n' consecutive true inputs; resets on false.
let debounce n =
    stream {
        let! ctx = getCtx()
        let! count = useState 0
        if ctx then count.Value <- count.Value + 1
        else count.Value <- 0
        return count.Value >= n
    }
