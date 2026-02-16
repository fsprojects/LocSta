module LocSta2.Tests.Dsp.Dynamics.HardClipTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Dynamics.HardClip

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``hardClip clips to threshold`` () =
    let result = hardClip 0.5 |> Eval.runWith [0.3; 0.8; -0.3; -0.8]
    assertApprox [0.3; 0.5; -0.3; -0.5] result
