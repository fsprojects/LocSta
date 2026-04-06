module LocSta.Blocks.TimeSeries.WindowAggregate

open LocSta.Core
open LocSta.TimeSeries

let private addToWindow (buf: ResizeArray<float>) n (value: float) =
    buf.Add(value)
    if buf.Count > n then buf.RemoveAt(0)

let private computeSum (buf: ResizeArray<float>) =
    let mutable s = 0.0
    for i = 0 to buf.Count - 1 do s <- s + buf[i]
    s

let private computeMin (buf: ResizeArray<float>) =
    let mutable m = buf[0]
    for i = 1 to buf.Count - 1 do
        if buf[i] < m then m <- buf[i]
    m

let private computeMax (buf: ResizeArray<float>) =
    let mutable m = buf[0]
    for i = 1 to buf.Count - 1 do
        if buf[i] > m then m <- buf[i]
    m

/// Maintains a sliding window of N DataPoints.
let window n =
    stream {
        let! ctx = getCtx<DataPoint<float>>()
        let! buf = useStateWith (fun () -> new ResizeArray<DataPoint<float>>(capacity = n))
        buf.Value.Add(ctx)
        if buf.Value.Count > n then buf.Value.RemoveAt(0)
        return buf.Value
    }

/// Sum of values in a sliding window of size N.
let windowSum n =
    stream {
        let! ctx = getCtx<DataPoint<float>>()
        let! buf = useStateWith (fun () -> new ResizeArray<float>(capacity = n))
        addToWindow buf.Value n ctx.Value
        return computeSum buf.Value
    }

/// Average of values in a sliding window of size N.
let windowAvg n =
    stream {
        let! ctx = getCtx<DataPoint<float>>()
        let! buf = useStateWith (fun () -> new ResizeArray<float>(capacity = n))
        addToWindow buf.Value n ctx.Value
        return computeSum buf.Value / float buf.Value.Count
    }

/// Minimum value in a sliding window of size N.
let windowMin n =
    stream {
        let! ctx = getCtx<DataPoint<float>>()
        let! buf = useStateWith (fun () -> new ResizeArray<float>(capacity = n))
        addToWindow buf.Value n ctx.Value
        return computeMin buf.Value
    }

/// Maximum value in a sliding window of size N.
let windowMax n =
    stream {
        let! ctx = getCtx<DataPoint<float>>()
        let! buf = useStateWith (fun () -> new ResizeArray<float>(capacity = n))
        addToWindow buf.Value n ctx.Value
        return computeMax buf.Value
    }

/// Count of values in a sliding window of size N.
let windowCount n =
    stream {
        let! _ = getCtx<DataPoint<float>>()
        let! count = useState 0
        count.Value <- System.Math.Min(count.Value + 1, n)
        return count.Value
    }

/// Cumulative sum that resets on boundary. Input: (DataPoint<float> * boundary: bool).
let intervalSum () =
    stream {
        let! (dp: DataPoint<float>, boundary: bool) = getCtx()
        let! acc = useState 0.0
        if boundary then acc.Value <- dp.Value
        else acc.Value <- acc.Value + dp.Value
        return acc.Value
    }

/// Cumulative sum over all received values.
let cumulativeSum () =
    stream {
        let! ctx = getCtx<DataPoint<float>>()
        let! acc = useState 0.0
        acc.Value <- acc.Value + ctx.Value
        return acc.Value
    }
