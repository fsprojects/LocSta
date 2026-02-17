module LocSta2.Tests.Counting.CountInTests

open Xunit
open LocSta
open LocSta.Blocks.Counting.CountIn

[<Fact>]
let ``countIn counts triggers in window`` () =
    let result = countIn 3 |> Eval.runWith [true; false; true; true; false]
    Assert.Equal<int list>([1; 1; 2; 2; 2], result)
