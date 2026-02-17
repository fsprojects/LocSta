module LocSta2.Tests.Statistics.RollingStdDevTests

open Xunit
open LocSta
open LocSta.Blocks.Statistics.RollingStdDev

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``rollingStdDev constant values give 0`` () =
    let result = rollingStdDev 3 |> Eval.runWith [5.0; 5.0; 5.0]
    assertApprox [0.0; 0.0; 0.0] result

[<Fact>]
let ``rollingStdDev basic computation`` () =
    let result = rollingStdDev 2 |> Eval.runWith [1.0; 3.0]
    assertApprox [0.0; 1.0] result
