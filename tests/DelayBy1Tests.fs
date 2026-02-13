module LocSta2.DelayBy1Tests

open Xunit
open LocSta
open LocSta.Blocks

[<Fact>]
let ``delayBy1 first sample returns default value`` () =
    let result =
        delayBy1 0
        |> Eval.runWith [10]
    Assert.Equal<int list>([0], result)

[<Fact>]
let ``delayBy1 delays input by one sample`` () =
    let result =
        delayBy1 0
        |> Eval.runWith [10; 20; 30; 40]
    Assert.Equal<int list>([0; 10; 20; 30], result)

[<Fact>]
let ``delayBy1 with string default`` () =
    let result =
        delayBy1 "none"
        |> Eval.runWith ["a"; "b"; "c"]
    Assert.Equal<string list>(["none"; "a"; "b"], result)

[<Fact>]
let ``delayBy1 single sample returns default`` () =
    let result =
        delayBy1 -1
        |> Eval.runWith [42]
    Assert.Equal<int list>([-1], result)

[<Fact>]
let ``delayBy1 with same values`` () =
    let result =
        delayBy1 0
        |> Eval.runWith [5; 5; 5]
    Assert.Equal<int list>([0; 5; 5], result)
