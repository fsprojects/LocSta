module LocSta2.Tests.Statistics.EmaTests

open Xunit
open LocSta
open LocSta.Blocks.Statistics.Ema

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``ema with alpha 1.0 passes through`` () =
    let result = ema 1.0 |> Eval.runWith [10.0; 20.0; 30.0]
    assertApprox [10.0; 20.0; 30.0] result

[<Fact>]
let ``ema with alpha 0.5 smooths`` () =
    let result = ema 0.5 |> Eval.runWith [10.0; 20.0; 30.0]
    assertApprox [10.0; 15.0; 22.5] result
