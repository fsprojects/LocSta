module LocSta2.Tests.Detection.CrossoverTests

open Xunit
open LocSta
open LocSta.Blocks.Detection.Crossover

[<Fact>]
let ``crossover detects cross above`` () =
    let result =
        crossover (ofSeq [1.0; 3.0; 5.0]) (ofSeq [4.0; 3.0; 2.0])
        |> Eval.run 3 (fun _ -> ())
    Assert.Equal<int list>([0; 0; 1], result)

[<Fact>]
let ``crossover detects cross below`` () =
    let result =
        crossover (ofSeq [5.0; 3.0; 1.0]) (ofSeq [2.0; 3.0; 4.0])
        |> Eval.run 3 (fun _ -> ())
    Assert.Equal<int list>([0; 0; -1], result)
