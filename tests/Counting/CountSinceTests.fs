module LocSta2.Tests.Counting.CountSinceTests

open Xunit
open LocSta
open LocSta.Blocks.Counting.CountSince

[<Fact>]
let ``countSince resets on trigger`` () =
    let result =
        countSince ()
        |> Eval.runWith [true; false; false; true; false; false; false]
    Assert.Equal<int list>([0; 1; 2; 0; 1; 2; 3], result)
