module LocSta2.Tests.Core.MultiEmitTests

open Xunit
open LocSta.Core

[<Fact>]
let ``yield emits single value`` () =
    let s = stream { yield 42 }
    let result = s |> Eval.run 3 (fun _ -> ())
    Assert.Equal<int list>([42; 42; 42], result)

[<Fact>]
let ``multiple yields emit multiple values per tick`` () =
    let s = stream {
        yield 1
        yield 2
        yield 3
    }
    let result = s |> Eval.run 6 (fun _ -> ())
    Assert.Equal<int list>([1; 2; 3; 1; 2; 3], result)

[<Fact>]
let ``yield and return behave the same for single value`` () =
    let withYield = stream { yield 10 } |> Eval.run 3 (fun _ -> ())
    let withReturn = stream { return 10 } |> Eval.run 3 (fun _ -> ())
    Assert.Equal<int list>(withReturn, withYield)

[<Fact>]
let ``yield! forwards all values from sub-stream`` () =
    let inner : SigStream<int,unit,unit> =
        fun s _ ->
            seq { 10; 20; 30 }, s
    let outer = stream {
        yield! inner
    }
    let result = outer |> Eval.run 6 (fun _ -> ())
    Assert.Equal<int list>([10; 20; 30; 10; 20; 30], result)

[<Fact>]
let ``yield mixed with let! and state`` () =
    let s = stream {
        let! ctx = getCtx()
        yield ctx
        yield ctx * 10
    }
    let result = s |> Eval.runWith [1; 2; 3]
    Assert.Equal<int list>([1; 10; 2; 20; 3; 30], result)

[<Fact>]
let ``conditional yield emits nothing on false branch`` () =
    let s = stream {
        let! ctx = getCtx()
        if ctx > 0 then
            yield ctx
    }
    let result = s |> Eval.runWith [1; -1; 2; -2; 3]
    Assert.Equal<int list>([1; 2; 3], result)

[<Fact>]
let ``stateful multi-emit with counter`` () =
    let s = stream {
        let! count = useState 0
        let c = count.Value
        count.Value <- c + 1
        yield c
        yield c * c
    }
    let result = s |> Eval.run 6 (fun _ -> ())
    Assert.Equal<int list>([0; 0; 1; 1; 2; 4], result)

[<Fact>]
let ``Eval.toSeq flattens multi-emit`` () =
    let s = stream {
        yield 1
        yield 2
    }
    let result = s |> Eval.toSeq (fun _ -> ()) |> Seq.take 6 |> Seq.toList
    Assert.Equal<int list>([1; 2; 1; 2; 1; 2], result)
