module LocSta.Blocks.TimeSeries.LookAhead

open LocSta.Core
open LocSta.TimeSeries

// LookAhead buffers N future values, delaying emission by N ticks.
// Once the buffer is full, it emits the oldest value paired with the lookahead values.
// Before the buffer fills, the Opt variants emit nothing; default variants emit immediately with defaults.

// --- Option variants (no output until buffer fills) ---

/// Buffers 1 ahead. Once filled, emits (current, next). Nothing before that.
let lookAhead1Opt () : SigStream<struct (DataPoint<'v> * DataPoint<'v>) voption, DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> ResizeArray<DataPoint<'v>>())
        buf.Value.Add(ctx)
        if buf.Value.Count > 1 then
            let current = buf.Value[0]
            let next = buf.Value[1]
            buf.Value.RemoveAt(0)
            return ValueSome struct (current, next)
        else
            return ValueNone
    }

/// Buffers 2 ahead. Once filled, emits (current, next1, next2). Nothing before that.
let lookAhead2Opt () : SigStream<struct (DataPoint<'v> * DataPoint<'v> * DataPoint<'v>) voption, DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> ResizeArray<DataPoint<'v>>())
        buf.Value.Add(ctx)
        if buf.Value.Count > 2 then
            let current = buf.Value[0]
            let n1 = buf.Value[1]
            let n2 = buf.Value[2]
            buf.Value.RemoveAt(0)
            return ValueSome struct (current, n1, n2)
        else
            return ValueNone
    }

/// Buffers 3 ahead. Once filled, emits (current, next1, next2, next3). Nothing before that.
let lookAhead3Opt () : SigStream<struct (DataPoint<'v> * DataPoint<'v> * DataPoint<'v> * DataPoint<'v>) voption, DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> ResizeArray<DataPoint<'v>>())
        buf.Value.Add(ctx)
        if buf.Value.Count > 3 then
            let current = buf.Value[0]
            let n1 = buf.Value[1]
            let n2 = buf.Value[2]
            let n3 = buf.Value[3]
            buf.Value.RemoveAt(0)
            return ValueSome struct (current, n1, n2, n3)
        else
            return ValueNone
    }

// --- Default-value variants (always emit, use defaults before buffer fills) ---

/// Returns (current, next) — uses defaultValue for next until buffer fills.
let lookAhead1 (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> ResizeArray<DataPoint<'v>>())
        buf.Value.Add(ctx)
        if buf.Value.Count > 1 then
            let current = buf.Value[0]
            let next = buf.Value[1]
            buf.Value.RemoveAt(0)
            return struct (current, next)
        else
            return struct (ctx, defaultValue)
    }

/// Returns (current, next1, next2) — uses defaultValue for unavailable slots.
let lookAhead2 (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> ResizeArray<DataPoint<'v>>())
        buf.Value.Add(ctx)
        if buf.Value.Count > 2 then
            let current = buf.Value[0]
            let n1 = buf.Value[1]
            let n2 = buf.Value[2]
            buf.Value.RemoveAt(0)
            return struct (current, n1, n2)
        else
            let current = buf.Value[0]
            let n1 = if buf.Value.Count > 1 then buf.Value[1] else defaultValue
            let n2 = defaultValue
            return struct (current, n1, n2)
    }

/// Returns (current, next1, next2, next3) — uses defaultValue for unavailable slots.
let lookAhead3 (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> ResizeArray<DataPoint<'v>>())
        buf.Value.Add(ctx)
        if buf.Value.Count > 3 then
            let current = buf.Value[0]
            let n1 = buf.Value[1]
            let n2 = buf.Value[2]
            let n3 = buf.Value[3]
            buf.Value.RemoveAt(0)
            return struct (current, n1, n2, n3)
        else
            let current = buf.Value[0]
            let n1 = if buf.Value.Count > 1 then buf.Value[1] else defaultValue
            let n2 = if buf.Value.Count > 2 then buf.Value[2] else defaultValue
            let n3 = defaultValue
            return struct (current, n1, n2, n3)
    }

// --- General N variants ---

/// Buffers N ahead. Once filled, emits (current, [next1..nextN]). Nothing before that.
let lookAheadOpt n : SigStream<(DataPoint<'v> * DataPoint<'v> list) voption, DataPoint<'v>, _> =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> new ResizeArray<DataPoint<'v>>(capacity = n + 1))
        buf.Value.Add(ctx)
        if buf.Value.Count > n then
            let current = buf.Value[0]
            let ahead = [for i in 1 .. n -> buf.Value[i]]
            buf.Value.RemoveAt(0)
            return ValueSome (current, ahead)
        else
            return ValueNone
    }

/// Returns (current, [next1..nextN]) — uses defaultValue for unavailable slots.
let lookAhead n (defaultValue: DataPoint<'v>) =
    stream {
        let! ctx = getCtx()
        let! buf = useStateWith (fun () -> new ResizeArray<DataPoint<'v>>(capacity = n + 1))
        buf.Value.Add(ctx)
        if buf.Value.Count > n then
            let current = buf.Value[0]
            let ahead = [for i in 1 .. n -> buf.Value[i]]
            buf.Value.RemoveAt(0)
            return (current, ahead)
        else
            let current = buf.Value[0]
            let ahead =
                [for i in 1 .. n ->
                    if i < buf.Value.Count then buf.Value[i]
                    else defaultValue]
            return (current, ahead)
    }
