module LocSta.Blocks.Dsp.Analysis.ZeroCrossRate

open LocSta.Core

/// Zero crossing rate over a window of 'windowSize' samples.
let zeroCrossRate windowSize =
    stream {
        let! signal = getCtx()
        let! window = useState []
        let newWindow = (signal :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        let crossings =
            newWindow
            |> List.pairwise
            |> List.sumBy (fun (a, b) -> if (a >= 0.0 && b < 0.0) || (a < 0.0 && b >= 0.0) then 1 else 0)
        return float crossings / float (max 1 (newWindow.Length - 1))
    }
