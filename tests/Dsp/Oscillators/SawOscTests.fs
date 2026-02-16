module LocSta2.Tests.Dsp.Oscillators.SawOscTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Oscillators.SawOsc

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``sawOsc ramps from -1 to 1`` () =
    // sr=4, freq=1 → phase increments by 0.25: 0, 0.25, 0.5, 0.75
    // values: 2*0-1=-1, 2*0.25-1=-0.5, 2*0.5-1=0, 2*0.75-1=0.5
    let result = sawOsc 4.0 |> Eval.runWith [1.0; 1.0; 1.0; 1.0]
    assertApprox [-1.0; -0.5; 0.0; 0.5] result
