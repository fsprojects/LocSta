module LocSta.Blocks.State.Changed

open LocSta.Core

/// Returns true whenever the input value differs from the previous one.
let inline changed () =
    stream {
        let! ctx = getCtx()
        let! st = useMemoWith (fun () -> MutableValue(ctx, false))
        let (prev, hasPrev) = st.Value
        let output = hasPrev && prev <> ctx
        st.Value <- (ctx, true)
        return output
    }
