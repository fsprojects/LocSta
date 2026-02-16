module LocSta2.Tests.Generators.CounterTests

open Xunit
open LocSta
open LocSta.Blocks.Generators.Counter

[<Fact>]
let ``counter 0 1 produces 0,1,2,3,4`` () =
    let result = counter 0 1 |> Eval.run 5 (fun _ -> ())
    Assert.Equal<int list>([0; 1; 2; 3; 4], result)

[<Fact>]
let ``counter 10 1 starts at 10`` () =
    let result = counter 10 1 |> Eval.run 4 (fun _ -> ())
    Assert.Equal<int list>([10; 11; 12; 13], result)

[<Fact>]
let ``counter 0 5 increments by 5`` () =
    let result = counter 0 5 |> Eval.run 4 (fun _ -> ())
    Assert.Equal<int list>([0; 5; 10; 15], result)

[<Fact>]
let ``counter 0 -1 decrements`` () =
    let result = counter 0 -1 |> Eval.run 4 (fun _ -> ())
    Assert.Equal<int list>([0; -1; -2; -3], result)

[<Fact>]
let ``counter 100 0 stays constant`` () =
    let result = counter 100 0 |> Eval.run 3 (fun _ -> ())
    Assert.Equal<int list>([100; 100; 100], result)

[<Fact>]
let ``counter single evaluation`` () =
    let result = counter 0 1 |> Eval.run 1 (fun _ -> ())
    Assert.Equal<int list>([0], result)

[<Fact>]
let ``counter 10 elements`` () =
    let result = counter 0 1 |> Eval.run 10 (fun _ -> ())
    Assert.Equal<int list>([0; 1; 2; 3; 4; 5; 6; 7; 8; 9], result)
