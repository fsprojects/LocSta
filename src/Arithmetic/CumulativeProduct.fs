module LocSta.Blocks.Arithmetic.CumulativeProduct

open LocSta.Core

/// Running product: accumulates the product of all input values starting from 'seed'.
let inline cumulativeProduct seed =
    stream {
        let! ctx = getCtx()
        let! prod = useState seed
        prod.Value <- prod.Value * ctx
        return prod.Value
    }
