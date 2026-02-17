module LocSta2.Tests.Dsp.Modulation.CrossfadeTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Modulation.Crossfade

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``crossfade mix 0 returns s1`` () =
    let result =
        crossfade (ofSeq [1.0; 1.0]) (ofSeq [0.0; 0.0]) (ofSeq [0.0; 0.0])
        |> Eval.run 2 (fun _ -> ())
    assertApprox [1.0; 1.0] result

[<Fact>]
let ``crossfade mix 1 returns s2`` () =
    let result =
        crossfade (ofSeq [1.0; 1.0]) (ofSeq [0.0; 0.0]) (ofSeq [1.0; 1.0])
        |> Eval.run 2 (fun _ -> ())
    assertApprox [0.0; 0.0] result

[<Fact>]
let ``crossfade mix 0.5 returns average`` () =
    let result =
        crossfade (ofSeq [1.0]) (ofSeq [0.0]) (ofSeq [0.5])
        |> Eval.run 1 (fun _ -> ())
    assertApprox [0.5] result
