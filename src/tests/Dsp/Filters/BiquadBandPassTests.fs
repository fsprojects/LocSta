module LocSta2.Tests.Dsp.Filters.BiquadBandPassTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Filters.BiquadBandPass

[<Fact>]
let ``biquadBandPass blocks DC`` () =
    let inputs = List.init 1000 (fun _ -> (1.0, 1000.0, 1.0))
    let result = biquadBandPass 44100.0 |> Eval.runWith inputs
    Assert.True(abs (List.last result) < 0.01)
