module LocSta2.Tests.Dsp.Filters.HighPass1Tests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Filters.HighPass1

[<Fact>]
let ``highPass1 blocks DC`` () =
    // Constant input should decay to 0
    let inputs = List.init 1000 (fun _ -> (1.0, 100.0))
    let result = highPass1 44100.0 |> Eval.runWith inputs
    Assert.True(abs (List.last result) < 0.01)
