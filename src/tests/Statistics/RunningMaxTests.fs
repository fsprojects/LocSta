module LocSta2.Tests.Statistics.RunningMaxTests

open Xunit
open LocSta
open LocSta.Blocks.Statistics.RunningMax

[<Fact>]
let ``runningMax tracks all-time max`` () =
    let result = runningMax () |> Eval.runWith [5; 3; 7; 1; 8]
    Assert.Equal<int list>([5; 5; 7; 7; 8], result)
