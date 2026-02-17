module LocSta.Blocks.Counting.CountIn

open LocSta.Core

/// Counts true values within a sliding window of 'windowSize' samples.
let countIn windowSize =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        return newWindow |> List.filter id |> List.length
    }
