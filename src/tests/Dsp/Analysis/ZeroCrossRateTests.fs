module LocSta2.Tests.Dsp.Analysis.ZeroCrossRateTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Analysis.ZeroCrossRate

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``zeroCrossRate of alternating signal is 1`` () =
    let result = zeroCrossRate 4 |> Eval.runWith [1.0; -1.0; 1.0; -1.0]
    // Last sample: window=[−1,1,−1,1], 3 crossings / 3 pairs = 1.0
    Assert.Equal(1.0, List.last result, 4)

[<Fact>]
let ``zeroCrossRate of constant signal is 0`` () =
    let result = zeroCrossRate 4 |> Eval.runWith [1.0; 1.0; 1.0; 1.0]
    Assert.Equal(0.0, List.last result, 4)
