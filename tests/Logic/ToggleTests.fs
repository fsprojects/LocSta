module LocSta2.Tests.Logic.ToggleTests

open Xunit
open LocSta
open LocSta.Blocks.Logic.Toggle

[<Fact>]
let ``toggle flips on trigger`` () =
    let result = toggle () |> Eval.runWith [true; false; true; true; false]
    Assert.Equal<bool list>([true; true; false; true; true], result)
