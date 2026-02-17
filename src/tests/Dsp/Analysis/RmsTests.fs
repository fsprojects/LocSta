module LocSta2.Tests.Dsp.Analysis.RmsTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Analysis.Rms

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``rms of constant signal equals absolute value`` () =
    let result = rms 4 |> Eval.runWith [1.0; 1.0; 1.0; 1.0]
    assertApprox [1.0; 1.0; 1.0; 1.0] result

[<Fact>]
let ``rms of alternating signal`` () =
    // RMS of [1, -1, 1, -1] = sqrt(1) = 1
    let result = rms 4 |> Eval.runWith [1.0; -1.0; 1.0; -1.0]
    assertApprox [1.0; 1.0; 1.0; 1.0] result
