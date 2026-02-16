module LocSta.Blocks.Dsp.Filters.DcBlock

open LocSta.Core

/// DC blocking filter. Removes DC offset from signal. R controls cutoff (~0.995 typical).
let dcBlock r =
    stream {
        let! signal = getCtx()
        let! prevInput = useStateWith (fun () -> signal)
        let! prevOutput = useState 0.0
        let output = signal - prevInput.Value + r * prevOutput.Value
        prevInput.Value <- signal
        prevOutput.Value <- output
        return output
    }
