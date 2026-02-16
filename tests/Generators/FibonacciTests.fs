module LocSta2.Tests.Generators.FibonacciTests

open Xunit
open LocSta
open LocSta.Blocks.Generators.Fibonacci

[<Fact>]
let ``fibonacci produces 1,2,3,5,8,13`` () =
    let result = fibonacci () |> Eval.run 6 (fun _ -> ())
    Assert.Equal<int list>([1; 2; 3; 5; 8; 13], result)
