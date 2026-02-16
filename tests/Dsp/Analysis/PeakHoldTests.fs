module LocSta2.Tests.Dsp.Analysis.PeakHoldTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Analysis.PeakHold

[<Fact>]
let ``peakHold captures peak`` () =
    let result = peakHold 0.99 |> Eval.runWith [0.5; 0.8; 0.3; 0.1]
    Assert.Equal(0.5, result.[0], 4)
    Assert.Equal(0.8, result.[1], 4)
    // After peak, should decay but stay above input
    Assert.True(result.[2] > 0.3)
    Assert.True(result.[3] > 0.1)

[<Fact>]
let ``peakHold with decay 0 holds forever`` () =
    let result = peakHold 0.0 |> Eval.runWith [1.0; 0.0; 0.0]
    Assert.Equal(1.0, result.[0], 4)
    Assert.Equal(0.0, result.[1], 4) // decay=0 means peak*0=0 after one step
