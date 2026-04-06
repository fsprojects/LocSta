module LocSta2.Tests.TimeSeries.EvalSafeguardTests

open Xunit
open LocSta.Core

// Eval.toSeq / Eval.run use an internal "while true" loop. If a stream
// permanently stops emitting values (e.g. an exhausted ofSeq), the loop
// would spin forever. The safeguard "maxSilentTicks" counts consecutive
// ticks that produce zero values and stops the sequence once the limit
// is reached.

[<Fact>]
let ``toSeqWith terminates after exhausted ofSeq`` () =
    // ofSeq [1;2] emits 1 on tick 0, 2 on tick 1, then Seq.empty forever.
    // With maxSilentTicks=5, the sequence stops after 5 empty ticks.
    //
    //   tick 0 → [1]       (silent = 0)
    //   tick 1 → [2]       (silent = 0)
    //   tick 2 → []        (silent = 1)
    //   tick 3 → []        (silent = 2)
    //   tick 4 → []        (silent = 3)
    //   tick 5 → []        (silent = 4)
    //   tick 6 → []        (silent = 5 → stop)
    //
    // Result: [1; 2]
    let s = ofSeq [1; 2]
    let result = s |> Eval.toSeqWith 5 (fun _ -> ()) |> Seq.toList
    Assert.Equal<int list>([1; 2], result)

[<Fact>]
let ``toSeq uses default 1000 silent ticks`` () =
    // Same principle as above, but with the default limit (1000).
    // After emitting [10; 20; 30], 1000 empty ticks pass → sequence ends.
    let s = ofSeq [10; 20; 30]
    let result = s |> Eval.toSeq (fun _ -> ()) |> Seq.toList
    Assert.Equal<int list>([10; 20; 30], result)

[<Fact>]
let ``safeguard does not interfere with normal infinite streams`` () =
    // A stream that always yields resets the silent counter every tick,
    // so it never hits the safeguard. We take 5 values to verify.
    //
    //   tick 0 → [42]  (silent = 0)
    //   tick 1 → [42]  (silent = 0)
    //   ...
    let s = stream { return 42 }
    let result = s |> Eval.toSeqWith 10 (fun _ -> ()) |> Seq.take 5 |> Seq.toList
    Assert.Equal<int list>([42; 42; 42; 42; 42], result)

[<Fact>]
let ``safeguard handles intermittent empty emissions`` () =
    // A stream that alternates between emitting and not emitting.
    // The silent counter resets on each emit, so it never triggers.
    //
    //   ctx=1 → yield 1    (silent = 0)
    //   ctx=0 → nothing    (silent = 1)
    //   ctx=2 → yield 2    (silent = 0)  ← reset!
    //   ctx=0 → nothing    (silent = 1)
    //   ctx=3 → yield 3    (silent = 0)  ← reset!
    let s = stream {
        let! ctx = getCtx()
        if ctx > 0 then yield ctx
    }
    let result = s |> Eval.runWith [1; 0; 2; 0; 3]
    Assert.Equal<int list>([1; 2; 3], result)
