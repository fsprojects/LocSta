module LocSta.Blocks.Statistics.RollingMax

open LocSta.Core

/// Maximum value within a sliding window of 'windowSize' samples.
let inline rollingMax windowSize =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        return newWindow |> List.max
    }
