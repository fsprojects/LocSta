module LocSta.Blocks.Dsp.Filters.DcBlock

open LocSta.Core

/// DC blocking filter. Removes DC offset from signal. R controls cutoff (~0.995 typical).
let dcBlock r =
    stream {
        let! signal = getCtx()
        let! st = useMemoWith (fun () -> MutableValue(signal, 0.0))
        let (prevInput, prevOutput) = st.Value
        let output = signal - prevInput + r * prevOutput
        st.Value <- (signal, output)
        return output
    }
