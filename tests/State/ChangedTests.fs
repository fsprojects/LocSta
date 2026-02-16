module LocSta2.Tests.State.ChangedTests

open Xunit
open LocSta
open LocSta.Blocks.State.Changed

[<Fact>]
let ``changed detects value changes`` () =
    let result = changed () |> Eval.runWith [1; 1; 2; 2; 3]
    Assert.Equal<bool list>([false; false; true; false; true], result)
