module LocSta2.Tests.Dsp.Filters.BiquadHighPassTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Filters.BiquadHighPass

[<Fact>]
let ``biquadHighPass blocks DC`` () =
    let inputs = List.init 1000 (fun _ -> (1.0, 100.0, 0.707))
    let result = biquadHighPass 44100.0 |> Eval.runWith inputs
    Assert.True(abs (List.last result) < 0.01)
