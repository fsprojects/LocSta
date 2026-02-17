module LocSta.Blocks.Statistics.RollingMin

open LocSta.Core

/// Minimum value within a sliding window of 'windowSize' samples.
let inline rollingMin windowSize =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        return newWindow |> List.min
    }
