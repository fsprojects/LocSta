module LocSta.Blocks.State.Edge

open LocSta.Core

/// Detects rising (1) and falling (-1) edges of a bool signal; 0 otherwise.
let edge () =
    stream {
        let! ctx = getCtx()
        let! prev = useState false
        let output =
            match prev.Value, ctx with
            | false, true -> 1
            | true, false -> -1
            | _ -> 0
        prev.Value <- ctx
        return output
    }
