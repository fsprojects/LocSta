module LocSta.Blocks.Counting.Rate

open LocSta.Core

/// Ratio of true values in a sliding window of 'windowSize' (0.0 to 1.0).
let rate windowSize =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        let count = newWindow |> List.filter id |> List.length
        return float count / float newWindow.Length
    }
