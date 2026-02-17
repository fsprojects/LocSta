module LocSta2.Tests.Counting.RateTests

open Xunit
open LocSta
open LocSta.Blocks.Counting.Rate

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``rate computes trigger rate in window`` () =
    let result = rate 4 |> Eval.runWith [true; false; true; true; false]
    assertApprox [1.0; 0.5; 2.0/3.0; 0.75; 0.5] result
