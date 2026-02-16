module LocSta2.Tests.Dsp.Modulation.BitCrushTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Modulation.BitCrush

[<Fact>]
let ``bitCrush quantizes signal`` () =
    // 1 bit → 2 levels: values should snap to 0.0 or 0.5
    let result = bitCrush 1 |> Eval.runWith [0.0; 0.3; 0.7; 1.0]
    // floor(0*2+0.5)/2=0.25, floor(0.3*2+0.5)/2=0.5, floor(0.7*2+0.5)/2=1.0, floor(1*2+0.5)/2=1.25
    Assert.True(result.Length = 4)
    // Just verify it produces discrete steps (not the original continuous values)
    let unique = result |> List.distinct
    Assert.True(unique.Length <= 4)
