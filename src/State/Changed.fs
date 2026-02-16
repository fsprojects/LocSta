module LocSta.Blocks.State.Changed

open LocSta.Core

/// Returns true whenever the input value differs from the previous one.
let inline changed () =
    stream {
        let! ctx = getCtx()
        let! prev = useState ValueNone
        let output =
            match prev.Value with
            | ValueNone -> false
            | ValueSome p -> p <> ctx
        prev.Value <- ValueSome ctx
        return output
    }
