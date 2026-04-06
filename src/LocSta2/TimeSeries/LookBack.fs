module LocSta.Blocks.TimeSeries.LookBack

open LocSta.Core
open LocSta.TimeSeries

// --- Option variants (ValueNone until buffer fills) ---

/// Returns the previous DataPoint, or ValueNone on the first tick.
let lookBack1Opt () : SigStream<DataPoint<'v> voption, DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! prev = useState ValueNone
        let output = prev.Value
        prev.Value <- ValueSome ctx
        return output
    }

/// Returns the previous 2 DataPoints as voption (ValueNone for unavailable slots).
let lookBack2Opt () : SigStream<struct (DataPoint<'v> voption * DataPoint<'v> voption), DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! buf = useState (ValueNone, ValueNone)
        let (p1, p2) = buf.Value
        buf.Value <- (ValueSome ctx, p1)
        return struct (p1, p2)
    }

/// Returns the previous 3 DataPoints as voption (ValueNone for unavailable slots).
let lookBack3Opt () : SigStream<struct (DataPoint<'v> voption * DataPoint<'v> voption * DataPoint<'v> voption), DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! buf = useState (ValueNone, ValueNone, ValueNone)
        let (p1, p2, p3) = buf.Value
        buf.Value <- (ValueSome ctx, p1, p2)
        return struct (p1, p2, p3)
    }

// --- Default-value variants (always emit) ---

/// Returns the previous DataPoint, or defaultValue on the first tick.
let lookBack1 (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! prev = useState defaultValue
        let output = prev.Value
        prev.Value <- ctx
        return output
    }

/// Returns the previous 2 DataPoints, padded with defaultValue.
let lookBack2 (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! buf = useState (defaultValue, defaultValue)
        let (p1, p2) = buf.Value
        buf.Value <- (ctx, p1)
        return struct (p1, p2)
    }

/// Returns the previous 3 DataPoints, padded with defaultValue.
let lookBack3 (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! buf = useState (defaultValue, defaultValue, defaultValue)
        let (p1, p2, p3) = buf.Value
        buf.Value <- (ctx, p1, p2)
        return struct (p1, p2, p3)
    }

// --- General N variants ---

let private buildOptResult (buf: ResizeArray<DataPoint<'v>>) n =
    [for i in 0 .. n - 1 do
        if i < buf.Count then ValueSome buf[i]
        else ValueNone]

let private buildDefaultResult (buf: ResizeArray<DataPoint<'v>>) n (defaultValue: DataPoint<'v>) =
    [for i in 0 .. n - 1 do
        if i < buf.Count then buf[i]
        else defaultValue]

let private shiftBuffer (buf: ResizeArray<DataPoint<'v>>) n (ctx: DataPoint<'v>) =
    if buf.Count >= n then buf.RemoveAt(n - 1)
    buf.Insert(0, ctx)

/// Returns previous N DataPoints as voption list (ValueNone for unavailable slots).
let lookBackOpt n : SigStream<DataPoint<'v> voption list, DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> new ResizeArray<DataPoint<'v>>(capacity = n))
        let result = buildOptResult buf.Value n
        shiftBuffer buf.Value n ctx
        return result
    }

/// Returns previous N DataPoints, padded with defaultValue.
let lookBack n (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> new ResizeArray<DataPoint<'v>>(capacity = n))
        let result = buildDefaultResult buf.Value n defaultValue
        shiftBuffer buf.Value n ctx
        return result
    }
