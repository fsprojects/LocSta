module LocSta2.Tests.Dsp.Oscillators.TriangleOscTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Oscillators.TriangleOsc

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``triangleOsc produces triangle wave`` () =
    // sr=4, freq=1 → phase 0, 0.25, 0.5, 0.75
    // tri(0)=2*|2*0-1|-1=2*1-1=1... wait let me recalc
    // f(p) = 2*|2p-1| - 1
    // p=0:   2*|0-1|-1 = 2*1-1 = 1  → but it wraps... hmm
    // Actually: p=0 → 2*|2*0-1|-1 = 2*1-1 = 1? No wait, that should be -1 at start.
    // p=0: 2*|-1|-1 = 2-1 = 1
    // p=0.25: 2*|-0.5|-1 = 1-1 = 0
    // p=0.5: 2*|0|-1 = -1
    // p=0.75: 2*|0.5|-1 = 0
    let result = triangleOsc 4.0 |> Eval.runWith [1.0; 1.0; 1.0; 1.0]
    assertApprox [1.0; 0.0; -1.0; 0.0] result
