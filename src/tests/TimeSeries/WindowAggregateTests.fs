module LocSta2.Tests.TimeSeries.WindowAggregateTests

open System
open Xunit
open LocSta.Core
open LocSta.TimeSeries
open LocSta.Blocks.TimeSeries.WindowAggregate

// Helper: create DataPoint with value v at second t
let dp v t = { Value = v; Timestamp = DateTimeOffset(2024, 1, 1, 0, 0, t, TimeSpan.Zero) }

// All window aggregations maintain a sliding window of the last N values.
// The window fills gradually (1, 2, ..., N elements) then slides.

// ============================================================================
// windowSum: Sum of values in a sliding window of size N.
//
//   n=3, inputs: 1, 2, 3, 4, 5
//
//   Tick   Input   Window      Sum
//   0      1.0     [1]         1.0
//   1      2.0     [1,2]       3.0
//   2      3.0     [1,2,3]     6.0     ← window full
//   3      4.0     [2,3,4]     9.0     ← 1 dropped, 4 added
//   4      5.0     [3,4,5]     12.0
// ============================================================================

[<Fact>]
let ``windowSum computes sliding sum`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4; dp 5.0 5]
    let result = windowSum 3 |> Eval.runWith inputs
    Assert.Equal<float list>([1.0; 3.0; 6.0; 9.0; 12.0], result)

// ============================================================================
// windowAvg: Average of values in a sliding window of size N.
//
//   n=3, inputs: 2, 4, 6
//
//   Tick   Input   Window      Avg
//   0      2.0     [2]         2.0 / 1 = 2.0
//   1      4.0     [2,4]       6.0 / 2 = 3.0
//   2      6.0     [2,4,6]     12.0 / 3 = 4.0
// ============================================================================

[<Fact>]
let ``windowAvg computes sliding average`` () =
    let inputs = [dp 2.0 1; dp 4.0 2; dp 6.0 3]
    let result = windowAvg 3 |> Eval.runWith inputs
    Assert.Equal<float list>([2.0; 3.0; 4.0], result)

// ============================================================================
// windowMin / windowMax: Tracks min/max over sliding window.
//
//   n=3, inputs: 3, 1, 5, 2
//
//   Tick   Input   Window     Min   Max
//   0      3.0     [3]        3.0   3.0
//   1      1.0     [3,1]      1.0   3.0
//   2      5.0     [3,1,5]    1.0   5.0
//   3      2.0     [1,5,2]    1.0   5.0    ← 3 dropped
// ============================================================================

[<Fact>]
let ``windowMin tracks minimum in sliding window`` () =
    let inputs = [dp 3.0 1; dp 1.0 2; dp 5.0 3; dp 2.0 4]
    let result = windowMin 3 |> Eval.runWith inputs
    Assert.Equal<float list>([3.0; 1.0; 1.0; 1.0], result)

[<Fact>]
let ``windowMax tracks maximum in sliding window`` () =
    let inputs = [dp 3.0 1; dp 1.0 2; dp 5.0 3; dp 2.0 4]
    let result = windowMax 3 |> Eval.runWith inputs
    Assert.Equal<float list>([3.0; 3.0; 5.0; 5.0], result)

// ============================================================================
// windowMin: Verify that old min drops off when it leaves the window.
//
//   n=2, inputs: 1, 5, 3
//
//   Tick   Input   Window     Min
//   0      1.0     [1]        1.0
//   1      5.0     [1,5]      1.0
//   2      3.0     [5,3]      3.0    ← 1 dropped, min is now 3
// ============================================================================

[<Fact>]
let ``windowMin drops old min when it leaves window`` () =
    let inputs = [dp 1.0 1; dp 5.0 2; dp 3.0 3]
    let result = windowMin 2 |> Eval.runWith inputs
    Assert.Equal(1.0, result[1])
    Assert.Equal(3.0, result[2])

// ============================================================================
// windowCount: Number of values currently in the window, saturates at N.
//
//   n=3, inputs: any 4 values
//
//   Tick   Count
//   0      1
//   1      2
//   2      3     ← saturated
//   3      3
// ============================================================================

[<Fact>]
let ``windowCount saturates at n`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = windowCount 3 |> Eval.runWith inputs
    Assert.Equal<int list>([1; 2; 3; 3], result)

// ============================================================================
// intervalSum: Cumulative sum that resets when boundary=true.
// On reset, the accumulator starts at the current value (not 0).
//
//   Inputs: (value, boundary)
//
//   Tick   Value   Boundary   Acc
//   0      1.0     false      0 + 1 = 1.0
//   1      2.0     false      1 + 2 = 3.0
//   2      3.0     true       3.0          ← reset to current value
//   3      4.0     false      3 + 4 = 7.0
//   4      5.0     true       5.0          ← reset to current value
// ============================================================================

[<Fact>]
let ``intervalSum resets on boundary`` () =
    let inputs = [
        (dp 1.0 1, false)
        (dp 2.0 2, false)
        (dp 3.0 3, true)
        (dp 4.0 4, false)
        (dp 5.0 5, true)
    ]
    let result = intervalSum () |> Eval.runWith inputs
    Assert.Equal<float list>([1.0; 3.0; 3.0; 7.0; 5.0], result)

// ============================================================================
// cumulativeSum: Running total over all received values (never resets).
//
//   Tick   Value   Cumulative
//   0      1.0     1.0
//   1      2.0     3.0
//   2      3.0     6.0
//   3      4.0     10.0
// ============================================================================

[<Fact>]
let ``cumulativeSum accumulates forever`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = cumulativeSum () |> Eval.runWith inputs
    Assert.Equal<float list>([1.0; 3.0; 6.0; 10.0], result)

// ============================================================================
// windowSum with n=1: Degenerate case — sum equals current value.
// ============================================================================

[<Fact>]
let ``windowSum with n=1 equals current value`` () =
    let inputs = [dp 5.0 1; dp 3.0 2; dp 7.0 3]
    let result = windowSum 1 |> Eval.runWith inputs
    Assert.Equal<float list>([5.0; 3.0; 7.0], result)

// ============================================================================
// window: Returns the raw sliding window as ResizeArray.
// Note: returns a mutable reference, so only the final state is testable.
// ============================================================================

[<Fact>]
let ``window maintains sliding DataPoint buffer`` () =
    let inputs = [dp 1.0 1; dp 2.0 2; dp 3.0 3; dp 4.0 4]
    let result = window 2 |> Eval.runWith inputs
    let last = result[3]
    Assert.Equal(2, last.Count)
    Assert.Equal(dp 3.0 3, last[0])
    Assert.Equal(dp 4.0 4, last[1])
