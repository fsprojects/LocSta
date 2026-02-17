module LocSta2.Tests.Statistics.RollingMaxTests

open Xunit
open LocSta
open LocSta.Blocks.Statistics.RollingMax

[<Fact>]
let ``rollingMax tracks windowed max`` () =
    let result = rollingMax 3 |> Eval.runWith [5; 3; 7; 1; 8]
    Assert.Equal<int list>([5; 5; 7; 7; 8], result)
