module LocSta.Blocks.Counting.CountWhere

open LocSta.Core

/// Counts how many times the predicate has been satisfied (all-time).
let inline countWhere ([<InlineIfLambda>] predicate) =
    stream {
        let! ctx = getCtx()
        let! count = useState 0
        if predicate ctx then count.Value <- count.Value + 1
        return count.Value
    }
