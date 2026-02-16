module LocSta.Blocks.Statistics.MovingAverage

open LocSta.Core

/// Simple moving average over the last 'windowSize' samples.
let movingAverage windowSize =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        return (newWindow |> List.sum |> float) / (float newWindow.Length)
    }
