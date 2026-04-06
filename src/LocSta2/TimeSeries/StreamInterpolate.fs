module LocSta.Blocks.TimeSeries.StreamInterpolate

open System
open LocSta.Core
open LocSta.TimeSeries

/// Sample-and-hold: holds the last known value. Input: (targetTimestamp, DataPoint<'v> voption).
/// Emits the held value at the target timestamp.
let sampleAndHold () =
    stream {
        let! (target: DateTimeOffset, incoming: DataPoint<'v> voption) = getCtx()
        let! held = useState ValueNone
        match incoming with
        | ValueSome dp -> held.Value <- ValueSome dp
        | ValueNone -> ()
        match held.Value with
        | ValueSome dp -> return ValueSome { Value = dp.Value; Timestamp = target }
        | ValueNone -> return ValueNone
    }

/// Nearest-neighbor interpolation. Holds the two closest points (before/after target).
/// Input: (targetTimestamp, DataPoint<float> voption).
let nearestNeighbor () =
    stream {
        let! (target: DateTimeOffset, incoming: DataPoint<float> voption) = getCtx()
        let initVal : DataPoint<float> voption = ValueNone
        let! state = useState (initVal, initVal)
        let (before, after) = state.Value
        let before, after =
            match incoming with
            | ValueSome dp ->
                if dp.Timestamp <= target then (ValueSome dp, after)
                else (before, ValueSome dp)
            | ValueNone -> (before, after)
        state.Value <- (before, after)
        match before, after with
        | ValueSome b, ValueSome a ->
            let distB = abs (target - b.Timestamp).TotalMilliseconds
            let distA = abs (target - a.Timestamp).TotalMilliseconds
            let v = if distB <= distA then b.Value else a.Value
            return ValueSome { Value = v; Timestamp = target }
        | ValueSome b, ValueNone ->
            return ValueSome { Value = b.Value; Timestamp = target }
        | ValueNone, ValueSome a ->
            return ValueSome { Value = a.Value; Timestamp = target }
        | ValueNone, ValueNone ->
            return ValueNone
    }

/// Linear interpolation between two surrounding points.
/// Input: (targetTimestamp, DataPoint<float> voption).
let linear () =
    stream {
        let! (target: DateTimeOffset, incoming: DataPoint<float> voption) = getCtx()
        let initVal : DataPoint<float> voption = ValueNone
        let! state = useState (initVal, initVal)
        let (before, after) = state.Value
        let before, after =
            match incoming with
            | ValueSome dp ->
                if dp.Timestamp <= target then (ValueSome dp, after)
                else (before, ValueSome dp)
            | ValueNone -> (before, after)
        state.Value <- (before, after)
        match before, after with
        | ValueSome b, ValueSome a ->
            let totalDt = (a.Timestamp - b.Timestamp).TotalMilliseconds
            let v =
                if totalDt = 0.0 then b.Value
                else
                    let dt = (target - b.Timestamp).TotalMilliseconds
                    b.Value + (dt / totalDt) * (a.Value - b.Value)
            return ValueSome { Value = v; Timestamp = target }
        | ValueSome b, ValueNone ->
            return ValueSome { Value = b.Value; Timestamp = target }
        | ValueNone, ValueSome a ->
            return ValueSome { Value = a.Value; Timestamp = target }
        | ValueNone, ValueNone ->
            return ValueNone
    }
