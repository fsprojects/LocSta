module LocSta2.Tests.Dsp.Oscillators.WhiteNoiseTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Oscillators.WhiteNoise

[<Fact>]
let ``whiteNoise produces values in -1 to 1 range`` () =
    let result = whiteNoise 42 |> Eval.run 100 (fun _ -> ())
    Assert.All(result, fun v -> Assert.InRange(v, -1.0, 1.0))

[<Fact>]
let ``whiteNoise with same seed is reproducible`` () =
    let r1 = whiteNoise 42 |> Eval.run 10 (fun _ -> ())
    let r2 = whiteNoise 42 |> Eval.run 10 (fun _ -> ())
    Assert.Equal<float list>(r1, r2)
