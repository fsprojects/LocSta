module LocSta2.Tests.TimeSeries.StreamInterpolateTests

open System
open Xunit
open LocSta.Core
open LocSta.TimeSeries
open LocSta.Blocks.TimeSeries.StreamInterpolate

// Helper: create timestamp at second s, DataPoint with value v at second s
let ts s = DateTimeOffset(2024, 1, 1, 0, 0, s, TimeSpan.Zero)
let dp v s = { Value = v; Timestamp = ts s }

// All interpolation blocks receive (targetTimestamp, incoming DataPoint voption).
// They maintain state (last known points) and produce interpolated values
// at the requested target timestamps.

// ============================================================================
// sampleAndHold: Holds the most recent value. When a new DataPoint arrives,
// it becomes the held value. When no new data arrives (ValueNone), the
// previously held value is returned at the target timestamp.
//
//   Tick   Target   Incoming           Held       Output
//   0      t=1s     ValueSome(10,t1)   dp(10,t1)  ValueSome{10, t=1s}
//   1      t=2s     ValueNone          dp(10,t1)  ValueSome{10, t=2s}  ← held
//   2      t=3s     ValueSome(20,t3)   dp(20,t3)  ValueSome{20, t=3s}
//   3      t=4s     ValueNone          dp(20,t3)  ValueSome{20, t=4s}  ← held
// ============================================================================

[<Fact>]
let ``sampleAndHold returns ValueNone with no data`` () =
    let inputs = [(ts 1, ValueNone); (ts 2, ValueNone)]
    let result = sampleAndHold () |> Eval.runWith inputs
    Assert.Equal(ValueNone, result[0])
    Assert.Equal(ValueNone, result[1])

[<Fact>]
let ``sampleAndHold holds last value across empty ticks`` () =
    let inputs = [
        (ts 1, ValueSome (dp 10.0 1))
        (ts 2, ValueNone)
        (ts 3, ValueSome (dp 20.0 3))
        (ts 4, ValueNone)
    ]
    let result = sampleAndHold () |> Eval.runWith inputs
    match result[0] with
    | ValueSome r -> Assert.Equal(10.0, r.Value)
    | ValueNone -> Assert.Fail("Expected ValueSome")
    match result[1] with
    | ValueSome r ->
        Assert.Equal(10.0, r.Value)
        Assert.Equal(ts 2, r.Timestamp)  // timestamp = target, not original
    | ValueNone -> Assert.Fail("Expected ValueSome")
    match result[2] with
    | ValueSome r -> Assert.Equal(20.0, r.Value)
    | ValueNone -> Assert.Fail("Expected ValueSome")
    match result[3] with
    | ValueSome r -> Assert.Equal(20.0, r.Value)
    | ValueNone -> Assert.Fail("Expected ValueSome")

// ============================================================================
// linear: Linear interpolation between the two surrounding points
// (one before, one after the target timestamp).
//
// Example: point A at t=0s val=0.0, point B at t=10s val=10.0
//          target at t=5s → interpolated = 5.0 (midpoint)
//
//   Timeline:  A(0,t0) -------- target(t5) -------- B(10,t10)
//   Fraction:  dt/totalDt = 5/10 = 0.5
//   Result:    0.0 + 0.5 * (10.0 - 0.0) = 5.0
// ============================================================================

[<Fact>]
let ``linear interpolates at midpoint`` () =
    let inputs = [
        (ts 5, ValueSome (dp 0.0 0))     // before point
        (ts 5, ValueSome (dp 10.0 10))    // after point
    ]
    let result = linear () |> Eval.runWith inputs
    match result[1] with
    | ValueSome r -> Assert.Equal(5.0, r.Value, 1e-10)
    | ValueNone -> Assert.Fail("Expected ValueSome")

[<Fact>]
let ``linear interpolates at quarter point`` () =
    // target=2.5s, A at t=0 val=0, B at t=10 val=100
    // fraction = 2.5/10 = 0.25 → result = 0 + 0.25 * 100 = 25.0
    let inputs = [
        (ts 2, ValueSome (dp 0.0 0))
        (ts 2, ValueSome (dp 100.0 10))
    ]
    let result = linear () |> Eval.runWith inputs
    match result[1] with
    | ValueSome r -> Assert.Equal(20.0, r.Value, 1e-10)  // 2/10 * 100
    | ValueNone -> Assert.Fail("Expected ValueSome")

[<Fact>]
let ``linear returns held value with only one point`` () =
    // With only a "before" point and no "after", it returns the before value.
    let inputs = [
        (ts 5, ValueSome (dp 42.0 3))
        (ts 10, ValueNone)
    ]
    let result = linear () |> Eval.runWith inputs
    match result[1] with
    | ValueSome r -> Assert.Equal(42.0, r.Value)
    | ValueNone -> Assert.Fail("Expected ValueSome")

// ============================================================================
// nearestNeighbor: Picks the value of the closest point (by timestamp).
// When equidistant, the "before" point wins (<=).
//
//   Timeline:  A(10,t0) --------- B(20,t10)
//   target t=3 → dist(A)=3, dist(B)=7 → picks A (value=10)
//   target t=5 → dist(A)=5, dist(B)=5 → picks A (tie → before wins)
//   target t=7 → dist(A)=7, dist(B)=3 → picks B (value=20)
// ============================================================================

[<Fact>]
let ``nearestNeighbor picks closest point`` () =
    let inputs = [
        (ts 3, ValueSome (dp 10.0 0))
        (ts 3, ValueSome (dp 20.0 10))
    ]
    let result = nearestNeighbor () |> Eval.runWith inputs
    match result[1] with
    | ValueSome r -> Assert.Equal(10.0, r.Value)  // t=0 is closer to t=3
    | ValueNone -> Assert.Fail("Expected ValueSome")

[<Fact>]
let ``nearestNeighbor picks before when equidistant`` () =
    let inputs = [
        (ts 5, ValueSome (dp 10.0 0))
        (ts 5, ValueSome (dp 20.0 10))
    ]
    let result = nearestNeighbor () |> Eval.runWith inputs
    match result[1] with
    | ValueSome r -> Assert.Equal(10.0, r.Value)  // tie → before wins (<=)
    | ValueNone -> Assert.Fail("Expected ValueSome")

[<Fact>]
let ``nearestNeighbor picks after when closer`` () =
    let inputs = [
        (ts 8, ValueSome (dp 10.0 0))
        (ts 8, ValueSome (dp 20.0 10))
    ]
    let result = nearestNeighbor () |> Eval.runWith inputs
    match result[1] with
    | ValueSome r -> Assert.Equal(20.0, r.Value)  // t=10 is closer to t=8
    | ValueNone -> Assert.Fail("Expected ValueSome")

[<Fact>]
let ``nearestNeighbor returns ValueNone without data`` () =
    let inputs = [(ts 5, ValueNone)]
    let result = nearestNeighbor () |> Eval.runWith inputs
    Assert.Equal(ValueNone, result[0])
