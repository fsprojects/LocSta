module LocSta2.Tests.Dsp.Envelope.AdsrTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Envelope.Adsr

[<Fact>]
let ``adsr idle produces zero`` () =
    let result = adsr 10.0 10.0 0.5 10.0 |> Eval.runWith [false; false; false]
    Assert.Equal<float list>([0.0; 0.0; 0.0], result)

[<Fact>]
let ``adsr gate on starts attack`` () =
    let result = adsr 10.0 10.0 0.5 10.0 |> Eval.runWith [false; true; true; true]
    // First gate=false → idle, then gate on → attack begins
    Assert.Equal(0.0, result.[0])
    Assert.True(result.[2] > 0.0)
