module LocSta.Blocks.Statistics.RollingStdDev

open LocSta.Core

/// Rolling standard deviation over the last 'windowSize' samples.
let rollingStdDev windowSize =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        let n = float newWindow.Length
        let mean = (newWindow |> List.sum) / n
        let variance =
            newWindow
            |> List.sumBy (fun x -> (x - mean) * (x - mean))
            |> fun s -> s / n
        return sqrt variance
    }
