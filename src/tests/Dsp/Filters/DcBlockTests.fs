module LocSta2.Tests.Dsp.Filters.DcBlockTests

open Xunit
open LocSta
open LocSta.Blocks.Dsp.Filters.DcBlock

[<Fact>]
let ``dcBlock removes DC offset`` () =
    let inputs = List.init 500 (fun _ -> 1.0)
    let result = dcBlock 0.995 |> Eval.runWith inputs
    Assert.True(abs (List.last result) < 0.05)

[<Fact>]
let ``dcBlock passes AC signal`` () =
    // Alternating signal should pass through
    let inputs = List.init 100 (fun i -> if i % 2 = 0 then 1.0 else -1.0)
    let result = dcBlock 0.995 |> Eval.runWith inputs
    Assert.True(abs (List.last result) > 0.5)
