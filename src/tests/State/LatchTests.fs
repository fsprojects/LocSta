module LocSta2.Tests.State.LatchTests

open Xunit
open LocSta
open LocSta.Blocks.State.Latch

[<Fact>]
let ``latch captures value on trigger`` () =
    let result =
        latch 0
        |> Eval.runWith [(true, 10); (false, 20); (false, 30); (true, 40); (false, 50)]
    Assert.Equal<int list>([10; 10; 10; 40; 40], result)

[<Fact>]
let ``latch returns default when no trigger`` () =
    let result = latch 99 |> Eval.runWith [(false, 1); (false, 2)]
    Assert.Equal<int list>([99; 99], result)
