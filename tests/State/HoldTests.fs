module LocSta2.Tests.State.HoldTests

open Xunit
open LocSta
open LocSta.Blocks.State.Hold

[<Fact>]
let ``hold captures value when predicate matches`` () =
    let result = hold (fun x -> x > 0) 0 |> Eval.runWith [1; -2; 3; -4; 5]
    Assert.Equal<int list>([1; 1; 3; 3; 5], result)

[<Fact>]
let ``hold returns default when no match`` () =
    let result = hold (fun x -> x > 100) 0 |> Eval.runWith [1; 2; 3]
    Assert.Equal<int list>([0; 0; 0], result)
