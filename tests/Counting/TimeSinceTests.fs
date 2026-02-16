module LocSta2.Tests.Counting.TimeSinceTests

open Xunit
open LocSta
open LocSta.Blocks.Counting.TimeSince

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``timeSince measures elapsed time`` () =
    let result =
        timeSince ()
        |> Eval.runWith [(true, 0.0); (false, 1.0); (false, 2.0); (true, 3.0); (false, 4.0)]
    assertApprox [0.0; 1.0; 2.0; 0.0; 1.0] result
