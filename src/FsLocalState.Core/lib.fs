namespace FsLocalState

[<AutoOpen>]
module Lib =

    // TODO: document each one of those
    module Gen =


        // --------
        // map / apply / transformation
        // --------

        let map2 (proj: 'v -> 's -> 'o) (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
            fun state ->
                [
                    for res in Gen.run inputGen state do
                        match res with
                        | Res.Emit (LoopEmit (v,s)) -> Res.Emit (LoopEmit (proj v s, s))
                        | Res.SkipWith (LoopSkip s) -> Res.SkipWith (LoopSkip s)
                        | Res.Stop -> Res.Stop
                ]
            |> Gen.createGen

        let map proj (inputGen: LoopGen<_,_>) =
            map2 (fun v _ -> proj v) inputGen

        let apply xGen fGen =
            loop {
                let! l' = xGen
                let! f' = fGen
                let result = f' l'
                yield result
            }

        /// Transforms a generator function to an effect function.    
        let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
            fun () -> gen


        // -------
        // count (that seems to be an important building block)
        // -------

        let inline count inclFrom step =
            feed {
                let! curr = Init inclFrom
                yield curr, curr + step
            }

        let inline countTo incFrom step inclusiveEnd =
            loop {
                let! c = count incFrom step
                match c <= inclusiveEnd with
                | true -> yield c
                | false -> return Loop.Stop
            }


        // ----------
        // reset / stop / count / ...
        // ----------

        let private emitFuncForMapValue = fun g v s -> [ Res.Emit (LoopEmit (v, s)) ]
        let private resetFuncForMapValue = fun g v s -> Gen.run g None
        let private stopFuncForMapValue = fun g v s -> [ Res.Stop ]

        /// Evluates the input gen and passes it's output to the predicate function:
        /// When that returns true, the input gen is evaluated once again with an empty state.
        /// It resurns the value and a bool indicating is a reset did happen.
        let whenFuncThen (pred: _ -> bool) onFalse onTrue (inputGen: LoopGen<_,_>) =
            fun state ->
                [
                    for res in Gen.run inputGen state do
                        match res with
                        | Res.Emit (LoopEmit (o,s)) ->
                            match pred o with
                            | false -> yield! onFalse inputGen o s
                            | true -> yield! onTrue inputGen o s
                        | Res.SkipWith (LoopSkip s) -> yield Res.SkipWith (LoopSkip s)
                        | Res.Stop -> yield Res.Stop
                ]
            |> Gen.createGen

        /// Evluates the input gen and passes it's output to the predicate function:
        /// When that returns true, the input gen is evaluated once again with an empty state.
        let whenFuncThenReset (pred: _ -> bool) (inputGen: LoopGen<_,_>) =
            whenFuncThen pred emitFuncForMapValue resetFuncForMapValue inputGen

        let whenFuncThenStop (pred: _ -> bool) (inputGen: LoopGen<_,_>) =
            whenFuncThen pred emitFuncForMapValue stopFuncForMapValue inputGen

        /// When the given predicate is true, the input gen is evaluated with an empty state.
        let whenValueThen (pred: bool) onTrue onFalse (inputGen: LoopGen<_,_>) =
            whenFuncThen (fun _ -> pred) onTrue onFalse inputGen

        /// When the given predicate is true, the input gen is evaluated with an empty state.
        let whenValueThenReset (pred: bool) (inputGen: LoopGen<_,_>) =
            whenValueThen pred emitFuncForMapValue resetFuncForMapValue inputGen

        let whenValueThenStop (pred: bool) (inputGen: LoopGen<_,_>) =
            whenValueThen pred emitFuncForMapValue stopFuncForMapValue inputGen

        let whenThen (pred: LoopGen<_,_>) onTrue onFalse (inputGen: LoopGen<_,_>) =
            loop {
                let! pred = pred
                return! whenValueThen pred onTrue onFalse inputGen
            }

        let whenThenReset (pred: LoopGen<_,_>) (inputGen: LoopGen<_,_>) =
            whenThen pred emitFuncForMapValue resetFuncForMapValue inputGen

        let whenThenStop (pred: LoopGen<_,_>) (inputGen: LoopGen<_,_>) =
            whenThen pred emitFuncForMapValue stopFuncForMapValue inputGen

        // TODO: doOnStop?

        // TODO: Docu: Stop means thet inputGen is *immediately* reevaluated (in this cycle; not in the next)
        let onStopThenReset (inputGen: LoopGen<_,_>) =
            let rec genFunc state =
                let g = Gen.run inputGen
                [
                    for res in g state do
                        match res with
                        | Res.Stop -> yield! genFunc None
                        | _ -> yield res
                ]
            Gen.createGen genFunc

        let inline repeatCount inclusiveStart increment inclusiveEnd =
            countTo inclusiveStart increment inclusiveEnd |> onStopThenReset

        let onCountThen count onTrue onFalse (inputGen: LoopGen<_,_>) =
            loop {
                let! c = repeatCount 0 1 (count - 1)
                return! whenValueThen (c = count) onTrue onFalse inputGen
            }

        let onCountThenReset count (inputGen: LoopGen<_,_>) =
            onCountThen count emitFuncForMapValue resetFuncForMapValue inputGen

        let onCountThenStop count (inputGen: LoopGen<_,_>) =
            onCountThen count emitFuncForMapValue stopFuncForMapValue inputGen
    
        // TODO: Test / Docu
        let includeState (inputGen: LoopGen<_,_>) =
            map2 (fun v s -> v,s) inputGen

        // TODO: Test / Docu
        let originalResult inputGen =
            fun state ->
                [
                    for res in Gen.run inputGen state do
                        match res with
                        | Res.Emit (LoopEmit (_,s)) as res ->
                            yield Res.Emit (LoopEmit (res, Some s))
                        | Res.SkipWith (LoopSkip s) as res ->
                            yield Res.Emit (LoopEmit (res, Some s))
                        | Res.Stop as res ->
                            yield Res.Emit (LoopEmit (res, state))
                ]
            |> Gen.createGen


        // ----------
        // accumulate, etc.
        // ----------

        let accumulate currentValue =
            feed {
                let! elements = Init []
                // TODO: Performance
                let newElements = elements @ [currentValue]
                yield newElements, newElements
            }

        let accumulateOnePart partLength currentValue =
            loop {
                let! c = count 0 1
                let! acc = accumulate currentValue
                if c = partLength - 1 then
                    // TODO Docu: Interessant - das "Stop" bedeutet nicht, dass die ganze Sequenz beendet wird, sondern
                    // es bedeutet: Wenn irgendwann diese Stelle nochmal evaluiert wird, DANN (und nicht vorher) wird gestoppt.
                    yield acc
                else if c = partLength then
                    return Loop.Stop
            }

        let accumulateManyParts count currentValue =
            accumulateOnePart count currentValue |> onStopThenReset

        // TODO: Maybe fork is so important that it could be implemenmted as an own builder
        let fork (inputGen: Gen<_,_>) =
            feed {
                let! runningStates = Init []
                let inputGen = Gen.run inputGen
                // TODO: Performance
                let forkResults =
                    runningStates @ [None]
                    |> List.map (fun forkState -> inputGen forkState |> Res.takeUntilStop)
                let emits =
                    forkResults
                    |> List.collect (fun aggRes -> Res.emittedValues aggRes.results)
                let newRunningStates =
                    forkResults
                    |> List.filter (fun res -> not res.isStopped)
                    |> List.map (fun res -> res.finalState)
                if emits.Length = 0 then
                    return Feed.SkipWith newRunningStates // in any case, emit the new state
                for e in emits do
                    // TODO: it would be really great to have an "Init" counterpart - a "Store"
                    // or something, so that the feedback state is set ONCE and not many times redundantly
                    // when yielding more than once
                    yield e, newRunningStates
            }

        // TODO: Test / Docu
        let windowed windowSize currentValue =
            accumulateOnePart windowSize currentValue |> fork


        // ----------
        // random / delay / slope
        // ----------
    
        // TODO: Implement a random number generator that exposes it's serializable state.
        let private dotnetRandom() = System.Random()
        let random () =
            feed {
                let! random = Init (dotnetRandom())
                yield random.NextDouble(), random
            }
    
        /// Delays a given value by 1 cycle.
        let delay1 value =
            fun state ->
                match state with
                | None -> [ Res.SkipWith (None, value) ]
                | Some delayed -> [ Res.Emit (delayed, value) ]
            |> Gen.createGen
    
        /// Delays a given value by n cycle.
        let delayn n value =
            failwith "TODO"
            //feed {
            //    let! initialValue = Init value
            //    return Feed.Feedback (initialValue, initialValue)
            //}

        // TODO: first (find)
        // TODO: Zip starting + current
    
        /// Positive slope.
        let inline slopePos input seed =
            feed {
                let! state = Init seed
                let res = state < input
                yield res, input
            }
    
        /// Negative slope.
        let inline slopeNeg input seed =
            feed {
                let! state = Init seed
                let res = state < input
                yield res, input
            }
    

        // ----------
        // other seq-like functions
        // ----------

        let head g = g |> Gen.toListn 1 |> List.exactlyOne

        let skip n g =
            loop {
                let! v = g
                let! c = count 0 1
                if c >= n then
                    yield v
            }

        // TODO: has "truncate" behaviour
        let take n g =
            loop {
                let! v = g
                let! c = count 0 1
                if c < n then
                    yield v
                else
                    return Loop.Stop
            }


open System.Runtime.CompilerServices

[<Extension>]
type Extensions() =

    [<Extension>]
    static member GetSlice(inputGen: LoopGen<'o, 's>, inclStartIdx, inclEndIdx) =
        let s = max 0 (defaultArg inclStartIdx 0)
        loop {
            let! i = Gen.count 0 1
            let! value = inputGen
            if i >= s then
                match inclEndIdx with
                | Some e when i > e ->
                    return Loop.Stop
                | _ ->
                    yield value
        }
