module LocSta2.Tests.Arithmetic.DiffTests

open Xunit
open LocSta
open LocSta.Blocks.Arithmetic.Diff

[<Fact>]
let ``diff computes difference from previous`` () =
    let result = diff 0 |> Eval.runWith [10; 12; 15; 13]
    Assert.Equal<int list>([10; 2; 3; -2], result)
