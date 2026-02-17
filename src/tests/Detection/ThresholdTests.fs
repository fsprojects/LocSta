module LocSta2.Tests.Detection.ThresholdTests

open Xunit
open LocSta
open LocSta.Blocks.Detection.Threshold

[<Fact>]
let ``threshold detects crossing level`` () =
    let result = threshold 3.0 |> Eval.runWith [1.0; 3.0; 5.0; 3.0; 1.0]
    Assert.Equal<int list>([0; 1; 0; 0; -1], result)
