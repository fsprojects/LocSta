module LocSta2.Tests.Dsp.Modulation.RingModTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Modulation.RingMod

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``ringMod multiplies two streams`` () =
    let result =
        ringMod (ofSeq [0.5; 1.0; -1.0]) (ofSeq [2.0; 3.0; 0.5])
        |> Eval.run 3 (fun _ -> ())
    assertApprox [1.0; 3.0; -0.5] result
