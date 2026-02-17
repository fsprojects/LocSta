module LocSta2.Tests.Counting.CountWhereTests

open Xunit
open LocSta
open LocSta.Blocks.Counting.CountWhere

[<Fact>]
let ``countWhere counts matching values`` () =
    let result = countWhere (fun x -> x > 0) |> Eval.runWith [1; -2; 3; -4; 5]
    Assert.Equal<int list>([1; 1; 2; 2; 3], result)
