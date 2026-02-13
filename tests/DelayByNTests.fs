module LocSta2.DelayByNTests

open Xunit
open LocSta
open LocSta.Blocks

[<Fact>]
let ``delayByN 1 behaves like delayBy1`` () =
    let result =
        delayByN 1 0
        |> Eval.runWith [10; 20; 30; 40]
    Assert.Equal<int list>([0; 10; 20; 30], result)

[<Fact>]
let ``delayByN 2 delays by two samples`` () =
    let result =
        delayByN 2 0
        |> Eval.runWith [10; 20; 30; 40; 50]
    Assert.Equal<int list>([0; 0; 10; 20; 30], result)

[<Fact>]
let ``delayByN 3 delays by three samples`` () =
    let result =
        delayByN 3 0
        |> Eval.runWith [1; 2; 3; 4; 5; 6]
    Assert.Equal<int list>([0; 0; 0; 1; 2; 3], result)

[<Fact>]
let ``delayByN 0 returns input immediately`` () =
    let result =
        delayByN 0 99
        |> Eval.runWith [1; 2; 3]
    Assert.Equal<int list>([1; 2; 3], result)

[<Fact>]
let ``delayByN with string values`` () =
    let result =
        delayByN 2 "x"
        |> Eval.runWith ["a"; "b"; "c"; "d"]
    Assert.Equal<string list>(["x"; "x"; "a"; "b"], result)
