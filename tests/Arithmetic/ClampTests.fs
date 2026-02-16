module LocSta2.Tests.Arithmetic.ClampTests

open Xunit
open LocSta
open LocSta.Blocks.Arithmetic.Clamp

[<Fact>]
let ``clamp constrains values`` () =
    let result = clamp 0 10 |> Eval.runWith [-5; 0; 5; 10; 15]
    Assert.Equal<int list>([0; 0; 5; 10; 10], result)
