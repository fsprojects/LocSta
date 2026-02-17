module LocSta2.Tests.Windowing.SegmentTests

open Xunit
open LocSta
open LocSta.Blocks.Windowing.Segment

[<Fact>]
let ``segment accumulates between boundaries`` () =
    let result =
        segment ()
        |> Eval.runWith [(1, false); (2, false); (3, true); (4, false); (5, true)]
    let expected: int list option list =
        [None; None; Some [1;2;3]; None; Some [4;5]]
    Assert.Equal<int list option list>(expected, result)
