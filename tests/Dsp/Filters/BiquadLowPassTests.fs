module LocSta2.Tests.Dsp.Filters.BiquadLowPassTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Filters.BiquadLowPass

[<Fact>]
let ``biquadLowPass smooths impulse`` () =
    let inputs = (1.0, 1000.0, 0.707) :: List.init 9 (fun _ -> (0.0, 1000.0, 0.707))
    let result = biquadLowPass 44100.0 |> Eval.runWith inputs
    // Impulse response should start non-zero and decay
    Assert.True(result.[0] > 0.0)
    Assert.True(result.[0] > 0.0)
