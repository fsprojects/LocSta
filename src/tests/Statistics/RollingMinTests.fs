module LocSta2.Tests.Statistics.RollingMinTests

open Xunit
open LocSta
open LocSta.Blocks.Statistics.RollingMin

[<Fact>]
let ``rollingMin tracks windowed min`` () =
    let result = rollingMin 3 |> Eval.runWith [5; 3; 7; 1; 8]
    Assert.Equal<int list>([5; 3; 3; 1; 1], result)
