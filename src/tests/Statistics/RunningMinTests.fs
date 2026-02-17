module LocSta2.Tests.Statistics.RunningMinTests

open Xunit
open LocSta
open LocSta.Blocks.Statistics.RunningMin

[<Fact>]
let ``runningMin tracks all-time min`` () =
    let result = runningMin () |> Eval.runWith [5; 3; 7; 1; 8]
    Assert.Equal<int list>([5; 3; 3; 1; 1], result)
