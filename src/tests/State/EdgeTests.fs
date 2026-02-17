module LocSta2.Tests.State.EdgeTests

open Xunit
open LocSta
open LocSta.Blocks.State.Edge

[<Fact>]
let ``edge detects rising and falling`` () =
    let result = edge () |> Eval.runWith [false; true; true; false; false; true]
    Assert.Equal<int list>([0; 1; 0; -1; 0; 1], result)
