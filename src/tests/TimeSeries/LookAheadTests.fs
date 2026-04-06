module LocSta2.Tests.TimeSeries.LookAheadTests

open System
open Xunit
open LocSta.Core
open LocSta.TimeSeries
open LocSta.Blocks.TimeSeries.LookAhead

// Helper: create DataPoint with value v at second t
let dp v t = { Value = v; Timestamp = DateTimeOffset(2024, 1, 1, 0, 0, t, TimeSpan.Zero) }
let dp0 = dp 0.0 0

// LookAhead buffers incoming values and delays emission.
// The Opt variants emit ValueNone until the buffer is full.
// The default variants emit immediately, using defaultValue for unknown future slots.

// ============================================================================
// lookAhead1Opt: Buffers 1 value ahead. Emits (current, next) once 2 values
// are available. Before that, emits ValueNone.
//
//   Tick   Input       Buffer         Output
//   0      dp(1,t1)    [dp1]          ValueNone         ← need 1 more
//   1      dp(2,t2)    [dp1,dp2]→[dp2] ValueSome(dp1, dp2) ← now we know next
//   2      dp(3,t3)    [dp2,dp3]→[dp3] ValueSome(dp2, dp3)
// ============================================================================

[<Fact>]
let ``lookAhead1Opt delays by 1 tick then emits pairs`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookAhead1Opt () |> Eval.runWith inputs
    Assert.Equal(ValueNone, result[0])
    match result[1] with
    | ValueSome struct (cur, next) ->
        Assert.Equal(dp 1.0 1, cur)
        Assert.Equal(dp 2.0 2, next)
    | ValueNone -> Assert.Fail("Expected ValueSome")
    match result[2] with
    | ValueSome struct (cur, next) ->
        Assert.Equal(dp 2.0 2, cur)
        Assert.Equal(dp 3.0 3, next)
    | ValueNone -> Assert.Fail("Expected ValueSome")

// ============================================================================
// lookAhead1: Same but always emits. Uses defaultValue for the unknown slot.
//
//   Tick   Input       Output (current, next)
//   0      dp(1,t1)    (dp1, dp0)     ← next unknown, use default
//   1      dp(2,t2)    (dp1, dp2)     ← buffer full, real values
//   2      dp(3,t3)    (dp2, dp3)
// ============================================================================

[<Fact>]
let ``lookAhead1 uses default before buffer fills`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3]
    let result = lookAhead1 dp0 |> Eval.runWith inputs
    let struct (cur, next) = result[0]
    Assert.Equal(dp 1.0 1, cur)
    Assert.Equal(dp0, next)
    let struct (cur, next) = result[1]
    Assert.Equal(dp 1.0 1, cur)
    Assert.Equal(dp 2.0 2, next)

// ============================================================================
// lookAhead2Opt: Buffers 2 ahead. Needs 3 values before first real emit.
//
//   Tick   Input       Buffer              Output
//   0      dp(1,t1)    [dp1]               ValueNone
//   1      dp(2,t2)    [dp1,dp2]           ValueNone
//   2      dp(3,t3)    [dp1,dp2,dp3]→[dp2,dp3]  ValueSome(dp1, dp2, dp3)
//   3      dp(4,t4)    [dp2,dp3,dp4]→[dp3,dp4]  ValueSome(dp2, dp3, dp4)
// ============================================================================

[<Fact>]
let ``lookAhead2Opt needs 3 values before first emit`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = lookAhead2Opt () |> Eval.runWith inputs
    Assert.Equal(ValueNone, result[0])
    Assert.Equal(ValueNone, result[1])
    match result[2] with
    | ValueSome struct (cur, n1, n2) ->
        Assert.Equal(dp 1.0 1, cur)
        Assert.Equal(dp 2.0 2, n1)
        Assert.Equal(dp 3.0 3, n2)
    | ValueNone -> Assert.Fail("Expected ValueSome")
    match result[3] with
    | ValueSome struct (cur, n1, n2) ->
        Assert.Equal(dp 2.0 2, cur)
        Assert.Equal(dp 3.0 3, n1)
        Assert.Equal(dp 4.0 4, n2)
    | ValueNone -> Assert.Fail("Expected ValueSome")

// ============================================================================
// lookAhead3: All 3 future slots use default until buffer fills.
//
//   Tick   Input       Output (current, next1, next2, next3)
//   0      dp(1,t1)    (dp1, dp0, dp0, dp0)    ← all 3 unknown
//   1      dp(2,t2)    (dp1, dp2, dp0, dp0)    ← 1 known, 2 default
// ============================================================================

[<Fact>]
let ``lookAhead3 pads all unknown slots with defaults`` () =
    let inputs = [dp 1.0 1; dp 2.0 2]
    let result = lookAhead3 dp0 |> Eval.runWith inputs
    let struct (cur, n1, n2, n3) = result[0]
    Assert.Equal(dp 1.0 1, cur)
    Assert.Equal(dp0, n1)
    Assert.Equal(dp0, n2)
    Assert.Equal(dp0, n3)
    let struct (cur, n1, n2, n3) = result[1]
    Assert.Equal(dp 1.0 1, cur)
    Assert.Equal(dp 2.0 2, n1)
    Assert.Equal(dp0, n2)
    Assert.Equal(dp0, n3)

// ============================================================================
// lookAheadOpt n: Generic variant with list output.
//
//   n=2:
//   Tick   Input       Output
//   0      dp(1,t1)    ValueNone
//   1      dp(2,t2)    ValueNone
//   2      dp(3,t3)    ValueSome (dp1, [dp2; dp3])
//   3      dp(4,t4)    ValueSome (dp2, [dp3; dp4])
// ============================================================================

[<Fact>]
let ``lookAheadOpt n emits once buffer fills`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = lookAheadOpt 2 |> Eval.runWith inputs
    Assert.Equal(ValueNone, result[0])
    Assert.Equal(ValueNone, result[1])
    match result[2] with
    | ValueSome (cur, ahead) ->
        Assert.Equal(dp 1.0 1, cur)
        Assert.Equal<DataPoint<float> list>([dp 2.0 2; dp 3.0 3], ahead)
    | ValueNone -> Assert.Fail("Expected ValueSome")
    match result[3] with
    | ValueSome (cur, ahead) ->
        Assert.Equal(dp 2.0 2, cur)
        Assert.Equal<DataPoint<float> list>([dp 3.0 3; dp 4.0 4], ahead)
    | ValueNone -> Assert.Fail("Expected ValueSome")

// ============================================================================
// lookAhead n: Generic variant with default padding.
//
//   n=2, default=dp0:
//   Tick   Input       Output (current, ahead list)
//   0      dp(1,t1)    (dp1, [dp0; dp0])
//   1      dp(2,t2)    (dp1, [dp2; dp0])     ← 1 known
//   2      dp(3,t3)    (dp1, [dp2; dp3])     ← full, dp1 emitted
//   3      dp(4,t4)    (dp2, [dp3; dp4])
// ============================================================================

[<Fact>]
let ``lookAhead n pads with defaults then slides`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = lookAhead 2 dp0 |> Eval.runWith inputs
    let (cur, ahead) = result[0]
    Assert.Equal(dp 1.0 1, cur)
    Assert.Equal<DataPoint<float> list>([dp0; dp0], ahead)
    let (cur, ahead) = result[2]
    Assert.Equal(dp 1.0 1, cur)
    Assert.Equal<DataPoint<float> list>([dp 2.0 2; dp 3.0 3], ahead)
    let (cur, ahead) = result[3]
    Assert.Equal(dp 2.0 2, cur)
    Assert.Equal<DataPoint<float> list>([dp 3.0 3; dp 4.0 4], ahead)
