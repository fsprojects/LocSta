module LocSta.Blocks.Dsp.Analysis.Rms

open LocSta.Core

/// RMS (Root Mean Square) level over a window of 'windowSize' samples.
let rms windowSize =
    stream {
        let! signal = getCtx()
        let! window = useState []
        let newWindow = (signal :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        let sumOfSquares = newWindow |> List.sumBy (fun x -> x * x)
        return sqrt (sumOfSquares / float newWindow.Length)
    }
