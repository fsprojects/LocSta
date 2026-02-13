module LocSta2.CounterTests

open Xunit
open LocSta

let counter start increment =
    stream {
        let! state = useState start
        let current = state.Value
        state.Value <- current + increment
        return current
    }

[<Fact>]
let ``counter 0 1 produces 0,1,2,3,4`` () =
    let result =
        counter 0 1
        |> Eval.toSeq (fun _ -> ())
        |> Seq.take 5
        |> Seq.toList
    Assert.Equal<int list>([0; 1; 2; 3; 4], result)

[<Fact>]
let ``counter 10 1 starts at 10`` () =
    let result =
        counter 10 1
        |> Eval.toSeq (fun _ -> ())
        |> Seq.take 4
        |> Seq.toList
    Assert.Equal<int list>([10; 11; 12; 13], result)

[<Fact>]
let ``counter 0 5 increments by 5`` () =
    let result =
        counter 0 5
        |> Eval.toSeq (fun _ -> ())
        |> Seq.take 4
        |> Seq.toList
    Assert.Equal<int list>([0; 5; 10; 15], result)

[<Fact>]
let ``counter 0 -1 decrements`` () =
    let result =
        counter 0 -1
        |> Eval.toSeq (fun _ -> ())
        |> Seq.take 4
        |> Seq.toList
    Assert.Equal<int list>([0; -1; -2; -3], result)

[<Fact>]
let ``counter 100 0 stays constant`` () =
    let result =
        counter 100 0
        |> Eval.toSeq (fun _ -> ())
        |> Seq.take 3
        |> Seq.toList
    Assert.Equal<int list>([100; 100; 100], result)

[<Fact>]
let ``counter single evaluation`` () =
    let result =
        counter 0 1
        |> Eval.toSeq (fun _ -> ())
        |> Seq.take 1
        |> Seq.toList
    Assert.Equal<int list>([0], result)

[<Fact>]
let ``counter 10 elements`` () =
    let result =
        counter 0 1
        |> Eval.toSeq (fun _ -> ())
        |> Seq.take 10
        |> Seq.toList
    Assert.Equal<int list>([0; 1; 2; 3; 4; 5; 6; 7; 8; 9], result)
