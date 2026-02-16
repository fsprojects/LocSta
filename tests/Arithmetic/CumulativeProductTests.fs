module LocSta2.Tests.Arithmetic.CumulativeProductTests

open Xunit
open LocSta
open LocSta.Blocks.Arithmetic.CumulativeProduct

[<Fact>]
let ``cumulativeProduct running product`` () =
    let result = cumulativeProduct 1 |> Eval.runWith [2; 3; 4]
    Assert.Equal<int list>([2; 6; 24], result)
