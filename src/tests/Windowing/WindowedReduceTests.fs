module LocSta2.Tests.Windowing.WindowedReduceTests

open Xunit
open LocSta
open LocSta.Blocks.Windowing.WindowedReduce

[<Fact>]
let ``windowedReduce sums window`` () =
    let result =
        windowedReduce 3 (fun acc x -> acc + x) 0
        |> Eval.runWith [1; 2; 3; 4; 5]
    Assert.Equal<int list>([1; 3; 6; 9; 12], result)
