module LocSta2.Tests.Dsp.Filters.LowPass1Tests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Filters.LowPass1

[<Fact>]
let ``lowPass1 smooths step input`` () =
    // Step from 0 to 1 should approach 1 gradually
    let inputs = (0.0, 1000.0) :: List.init 99 (fun _ -> (1.0, 1000.0))
    let result = lowPass1 44100.0 |> Eval.runWith inputs
    // First sample should be 0 (initialized to first input)
    Assert.Equal(0.0, result.[0], 4)
    // Should converge toward 1.0
    Assert.True(List.last result > result.[1])

[<Fact>]
let ``lowPass1 with very high cutoff passes signal through`` () =
    let inputs = [(1.0, 20000.0); (0.0, 20000.0); (1.0, 20000.0)]
    let result = lowPass1 44100.0 |> Eval.runWith inputs
    // Very high cutoff relative to SR → near pass-through
    Assert.True(result.[0] > 0.7)
