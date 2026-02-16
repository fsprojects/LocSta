module LocSta2.Tests.Dsp.Dynamics.SoftClipTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Dynamics.SoftClip

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``softClip zero passes through`` () =
    let result = softClip 1.0 |> Eval.runWith [0.0]
    assertApprox [0.0] result

[<Fact>]
let ``softClip limits large values`` () =
    let result = softClip 1.0 |> Eval.runWith [100.0]
    // tanh(100) ≈ 1.0
    Assert.True(result.[0] > 0.99 && result.[0] <= 1.0)

[<Fact>]
let ``softClip is symmetric`` () =
    let pos = softClip 1.0 |> Eval.runWith [2.0]
    let neg = softClip 1.0 |> Eval.runWith [-2.0]
    Assert.Equal(pos.[0], -neg.[0], 4)
