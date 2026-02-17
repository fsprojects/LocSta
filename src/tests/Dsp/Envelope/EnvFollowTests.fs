module LocSta2.Tests.Dsp.Envelope.EnvFollowTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Envelope.EnvFollow

[<Fact>]
let ``envFollow tracks signal amplitude`` () =
    // Loud signal → envelope rises, then silence → envelope falls
    let loud = List.init 100 (fun _ -> 1.0)
    let silent = List.init 100 (fun _ -> 0.0)
    let result = envFollow 44100.0 10.0 50.0 |> Eval.runWith (loud @ silent)
    // After loud section, envelope should be high
    Assert.True(result.[99] > 0.0 && result.[50] < result.[99])
    // After silent section, envelope should have decayed
    Assert.True(result.[199] < result.[99])
