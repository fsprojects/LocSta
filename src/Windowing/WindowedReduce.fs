module LocSta.Blocks.Windowing.WindowedReduce

open LocSta.Core

/// Applies a fold over a sliding window of 'windowSize' samples.
let inline windowedReduce windowSize ([<InlineIfLambda>] folder) seed =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        return newWindow |> List.rev |> List.fold folder seed
    }
