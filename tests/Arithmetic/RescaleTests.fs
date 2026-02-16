module LocSta2.Tests.Arithmetic.RescaleTests

open Xunit
open LocSta
open LocSta.Blocks.Arithmetic.Rescale

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``rescale maps range`` () =
    let result = rescale 0.0 1.0 0.0 100.0 |> Eval.runWith [0.0; 0.25; 0.5; 1.0]
    assertApprox [0.0; 25.0; 50.0; 100.0] result
