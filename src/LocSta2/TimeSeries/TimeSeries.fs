namespace LocSta.TimeSeries

open System

[<Struct>]
type DataPoint<'v> = {
    Value: 'v
    Timestamp: DateTimeOffset
}

[<Struct>]
type ResampleContext<'v> = {
    Timestamp: DateTimeOffset
    Window: DataPoint<'v> array
    Before: DataPoint<'v> voption
    After: DataPoint<'v> voption
}

type Resampler<'v, 'r> = ResampleContext<'v> -> 'r

module Aggregate =

    let last : Resampler<'v, 'v option> =
        fun ctx ->
            let w = ctx.Window
            if w.Length > 0 then Some w[w.Length - 1].Value else None

    let first : Resampler<'v, 'v option> =
        fun ctx ->
            let w = ctx.Window
            if w.Length > 0 then Some w[0].Value else None

    let count : Resampler<'v, int> =
        fun ctx -> ctx.Window.Length

    let inline sum () =
        fun (ctx: ResampleContext< ^v>) ->
            let w = ctx.Window
            if w.Length = 0 then None
            else
                let mutable acc = w[0].Value
                for i = 1 to w.Length - 1 do
                    acc <- acc + w[i].Value
                Some acc

    let inline avg () =
        fun (ctx: ResampleContext< ^v>) ->
            let w = ctx.Window
            if w.Length = 0 then None
            else
                let mutable s = 0.0
                for i = 0 to w.Length - 1 do
                    s <- s + float w[i].Value
                Some (s / float w.Length)

    let inline min () =
        fun (ctx: ResampleContext< ^v>) ->
            let w = ctx.Window
            if w.Length = 0 then None
            else
                let mutable m = w[0].Value
                for i = 1 to w.Length - 1 do
                    if w[i].Value < m then m <- w[i].Value
                Some m

    let inline max () =
        fun (ctx: ResampleContext< ^v>) ->
            let w = ctx.Window
            if w.Length = 0 then None
            else
                let mutable m = w[0].Value
                for i = 1 to w.Length - 1 do
                    if w[i].Value > m then m <- w[i].Value
                Some m

module Interpolate =

    let sampleAndHold : Resampler<'v, 'v option> =
        fun ctx ->
            let w = ctx.Window
            if w.Length > 0 then Some w[w.Length - 1].Value
            else
                match ctx.Before with
                | ValueSome dp -> Some dp.Value
                | ValueNone -> None

    let nearestNeighbor : Resampler<'v, 'v option> =
        fun ctx ->
            let w = ctx.Window
            let mutable beforeDp = ctx.Before
            let mutable afterDp = ctx.After
            let mutable i = 0
            while i < w.Length && w[i].Timestamp <= ctx.Timestamp do
                beforeDp <- ValueSome w[i]
                i <- i + 1
            if i < w.Length then
                afterDp <- ValueSome w[i]
            match beforeDp, afterDp with
            | ValueSome b, ValueSome a ->
                let distB = abs (ctx.Timestamp - b.Timestamp).TotalMilliseconds
                let distA = abs (ctx.Timestamp - a.Timestamp).TotalMilliseconds
                if distB <= distA then Some b.Value else Some a.Value
            | ValueSome b, ValueNone -> Some b.Value
            | ValueNone, ValueSome a -> Some a.Value
            | ValueNone, ValueNone -> None

    let linear : Resampler<float, float option> =
        fun ctx ->
            let w = ctx.Window
            let mutable beforeDp = ctx.Before
            let mutable afterDp = ctx.After
            let mutable i = 0
            while i < w.Length && w[i].Timestamp <= ctx.Timestamp do
                beforeDp <- ValueSome w[i]
                i <- i + 1
            if i < w.Length then
                afterDp <- ValueSome w[i]
            match beforeDp, afterDp with
            | ValueSome b, ValueSome a ->
                let totalDt = (a.Timestamp - b.Timestamp).TotalMilliseconds
                if totalDt = 0.0 then Some b.Value
                else
                    let dt = (ctx.Timestamp - b.Timestamp).TotalMilliseconds
                    Some (b.Value + (dt / totalDt) * (a.Value - b.Value))
            | ValueSome b, ValueNone -> Some b.Value
            | ValueNone, ValueSome a -> Some a.Value
            | ValueNone, ValueNone -> None
