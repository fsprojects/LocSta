module LocSta2.Tests.Dsp.Dynamics.GateTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Dynamics.Gate

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``gate silences below threshold`` () =
    let inputs = [(0.5, 0.8); (0.5, 0.3); (0.5, 0.6)]
    let result = gate 0.5 |> Eval.runWith inputs
    assertApprox [0.5; 0.0; 0.5] result
