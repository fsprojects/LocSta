module LocSta2.Tests.Logic.DebounceTests

open Xunit
open LocSta
open LocSta.Blocks.Logic.Debounce

[<Fact>]
let ``debounce requires n consecutive trues`` () =
    let result =
        debounce 3
        |> Eval.runWith [true; true; false; true; true; true; true]
    Assert.Equal<bool list>([false; false; false; false; false; true; true], result)
