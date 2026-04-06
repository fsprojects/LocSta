module LocSta2.Tests.TimeSeries.LookBackTests

open System
open Xunit
open LocSta.Core
open LocSta.TimeSeries
open LocSta.Blocks.TimeSeries.LookBack

// Helper: create DataPoint with value v at second t
let dp v t = { Value = v; Timestamp = DateTimeOffset(2024, 1, 1, 0, 0, t, TimeSpan.Zero) }
let dp0 = dp 0.0 0

// ============================================================================
// lookBack1Opt: Returns the previous DataPoint as ValueSome, or ValueNone
// on the first tick (no previous value available yet).
//
//   Tick   Input       Output
//   0      dp(1,t1)    ValueNone          ← no previous value
//   1      dp(2,t2)    ValueSome dp(1,t1) ← previous was dp(1,t1)
//   2      dp(3,t3)    ValueSome dp(2,t2) ← previous was dp(2,t2)
// ============================================================================

[<Fact>]
let ``lookBack1Opt returns ValueNone on first tick, then previous`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookBack1Opt () |> Eval.runWith inputs
    Assert.Equal(3, result.Length)
    Assert.Equal(ValueNone, result[0])
    Assert.Equal(ValueSome (dp 1.0 1), result[1])
    Assert.Equal(ValueSome (dp 2.0 2), result[2])

// ============================================================================
// lookBack1: Same as lookBack1Opt, but returns a default value instead of
// ValueNone on the first tick.
//
//   Tick   Input       Output
//   0      dp(1,t1)    dp(0,t0)   ← default value
//   1      dp(2,t2)    dp(1,t1)   ← previous
//   2      dp(3,t3)    dp(2,t2)   ← previous
// ============================================================================

[<Fact>]
let ``lookBack1 returns default on first tick`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookBack1 dp0 |> Eval.runWith inputs
    Assert.Equal(dp0, result[0])
    Assert.Equal(dp 1.0 1, result[1])
    Assert.Equal(dp 2.0 2, result[2])

// ============================================================================
// lookBack2Opt: Returns the previous 2 DataPoints as a struct tuple of
// voptions. Slots fill gradually.
//
//   Tick   Input       Output (prev1, prev2)
//   0      dp(1,t1)    (ValueNone,          ValueNone)
//   1      dp(2,t2)    (ValueSome dp(1,t1), ValueNone)
//   2      dp(3,t3)    (ValueSome dp(2,t2), ValueSome dp(1,t1))
// ============================================================================

[<Fact>]
let ``lookBack2Opt fills slots gradually`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookBack2Opt () |> Eval.runWith inputs
    let struct (a, b) = result[0]
    Assert.Equal(ValueNone, a)
    Assert.Equal(ValueNone, b)
    let struct (a, b) = result[1]
    Assert.Equal(ValueSome (dp 1.0 1), a)
    Assert.Equal(ValueNone, b)
    let struct (a, b) = result[2]
    Assert.Equal(ValueSome (dp 2.0 2), a)
    Assert.Equal(ValueSome (dp 1.0 1), b)

// ============================================================================
// lookBack2: Returns the previous 2 DataPoints, padded with defaultValue.
//
//   Tick   Input       Output (prev1, prev2)
//   0      dp(1,t1)    (dp0, dp0)              ← both default
//   1      dp(2,t2)    (dp(1,t1), dp0)         ← one real, one default
//   2      dp(3,t3)    (dp(2,t2), dp(1,t1))    ← both real
// ============================================================================

[<Fact>]
let ``lookBack2 pads with default, then fills`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookBack2 dp0 |> Eval.runWith inputs
    let struct (a, b) = result[0]
    Assert.Equal(dp0, a)
    Assert.Equal(dp0, b)
    let struct (a, b) = result[1]
    Assert.Equal(dp 1.0 1, a)
    Assert.Equal(dp0, b)
    let struct (a, b) = result[2]
    Assert.Equal(dp 2.0 2, a)
    Assert.Equal(dp 1.0 1, b)

// ============================================================================
// lookBack3: Returns the previous 3 DataPoints (padded with default).
//
//   Tick   Input       Output (prev1, prev2, prev3)
//   0      dp(1,t1)    (dp0, dp0, dp0)
//   1      dp(2,t2)    (dp(1,t1), dp0, dp0)
//   2      dp(3,t3)    (dp(2,t2), dp(1,t1), dp0)
//   3      dp(4,t4)    (dp(3,t3), dp(2,t2), dp(1,t1))  ← fully filled
// ============================================================================

[<Fact>]
let ``lookBack3 returns 3 previous values`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = lookBack3 dp0 |> Eval.runWith inputs
    let struct (a, b, c) = result[3]
    Assert.Equal(dp 3.0 3, a)
    Assert.Equal(dp 2.0 2, b)
    Assert.Equal(dp 1.0 1, c)

// ============================================================================
// lookBackOpt n: Generic N variant. Returns a list of N voptions.
// Same gradual-fill behavior as the fixed variants.
//
//   n=2:
//   Tick   Input       Output
//   0      dp(1,t1)    [ValueNone; ValueNone]
//   1      dp(2,t2)    [ValueSome dp(1,t1); ValueNone]
//   2      dp(3,t3)    [ValueSome dp(2,t2); ValueSome dp(1,t1)]
// ============================================================================

[<Fact>]
let ``lookBackOpt n returns voption list that fills gradually`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookBackOpt 2 |> Eval.runWith inputs
    Assert.Equal<DataPoint<float> voption list>([ValueNone; ValueNone], result[0])
    Assert.Equal<DataPoint<float> voption list>([ValueSome (dp 1.0 1); ValueNone], result[1])
    Assert.Equal<DataPoint<float> voption list>([ValueSome (dp 2.0 2); ValueSome (dp 1.0 1)], result[2])

// ============================================================================
// lookBack n: Generic N variant with default padding.
//
//   n=2, default=dp0:
//   Tick   Input       Output
//   0      dp(1,t1)    [dp0; dp0]
//   1      dp(2,t2)    [dp(1,t1); dp0]
//   2      dp(3,t3)    [dp(2,t2); dp(1,t1)]
// ============================================================================

[<Fact>]
let ``lookBack n returns padded list`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookBack 2 dp0 |> Eval.runWith inputs
    Assert.Equal<DataPoint<float> list>([dp0; dp0], result[0])
    Assert.Equal<DataPoint<float> list>([dp 1.0 1; dp0], result[1])
    Assert.Equal<DataPoint<float> list>([dp 2.0 2; dp 1.0 1], result[2])

// ============================================================================
// lookBack n: Verify sliding behavior — older values drop off.
//
//   n=2, default=dp0:
//   Tick   Input       Buffer after shift    Output (before shift)
//   0      dp(1,t1)    [dp(1,t1)]            [dp0; dp0]
//   1      dp(2,t2)    [dp(2,t2); dp(1,t1)]  [dp(1,t1); dp0]
//   2      dp(3,t3)    [dp(3,t3); dp(2,t2)]  [dp(2,t2); dp(1,t1)]
//   3      dp(4,t4)    [dp(4,t4); dp(3,t3)]  [dp(3,t3); dp(2,t2)]  ← dp(1) gone
// ============================================================================

[<Fact>]
let ``lookBack n slides window — old values drop off`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = lookBack 2 dp0 |> Eval.runWith inputs
    Assert.Equal<DataPoint<float> list>([dp 3.0 3; dp 2.0 2], result[3])
