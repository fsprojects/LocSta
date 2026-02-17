module LocSta2.Tests.Dsp.Oscillators.SquareOscTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Oscillators.SquareOsc

let assertApprox (expected: float list) (actual: float list) =
    Assert.Equal(expected.Length, actual.Length)
    (expected, actual) ||> List.iter2 (fun e a -> Assert.Equal(e, a, 4))

[<Fact>]
let ``squareOsc 50% duty cycle`` () =
    // sr=4, freq=1, pw=0.5 → phase 0,0.25,0.5,0.75
    // 0<0.5→1, 0.25<0.5→1, 0.5>=0.5→-1, 0.75>=0.5→-1
    let result = squareOsc 4.0 |> Eval.runWith [(1.0, 0.5); (1.0, 0.5); (1.0, 0.5); (1.0, 0.5)]
    assertApprox [1.0; 1.0; -1.0; -1.0] result
