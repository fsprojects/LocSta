module LocSta2.Tests.Arithmetic.CumulativeSumTests

open Xunit
open LocSta
open LocSta.Blocks.Arithmetic.CumulativeSum

[<Fact>]
let ``cumulativeSum running total`` () =
    let result = cumulativeSum 0 |> Eval.runWith [1; 2; 3; 4]
    Assert.Equal<int list>([1; 3; 6; 10], result)
