module LocSta2.Tests.Dsp.Oscillators.SineOscTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Oscillators.SineOsc

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``sineOsc at sampleRate/4 frequency produces 0, 1, 0, -1 pattern`` () =
    // 4 samples per cycle at sr=4, freq=1
    let result = sineOsc 4.0 |> Eval.runWith [1.0; 1.0; 1.0; 1.0]
    assertApprox [0.0; 1.0; 0.0; -1.0] result

[<Fact>]
let ``sineOsc starts at zero`` () =
    let result = sineOsc 44100.0 |> Eval.runWith [440.0]
    assertApprox [0.0] result
