module LocSta.Blocks.Arithmetic.CumulativeSum

open LocSta.Core

/// Running total: accumulates the sum of all input values starting from 'seed'.
let inline cumulativeSum seed =
    stream {
        let! ctx = getCtx()
        let! sum = useState seed
        sum.Value <- sum.Value + ctx
        return sum.Value
    }
