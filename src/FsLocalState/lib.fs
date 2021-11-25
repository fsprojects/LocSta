namespace FsLocalState

[<AutoOpen>]
module Lib =

    // TODO: document each one of those
    module Gen =


        // --------
        // map / apply / transformation
        // --------

        let map2 (proj: 'v -> 's option -> 'o) (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
            fun state ->
                let mapValues values state = [ for v in values do proj v state ]
                match Gen.run inputGen state with
                | Res.Continue (values, LoopState s) ->
                    Res.Continue (mapValues values s, LoopState s)
                | Res.Stop values -> Res.Stop (mapValues values None)
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

        let private resetFuncForMapValue = fun gen values s -> Gen.run gen None
        let private stopFuncForMapValue = fun gen values s -> Res.Stop values

        /// Evluates the input gen and passes it's output to the predicate function:
        /// When that returns true, the input gen is evaluated once again with an empty state.
        /// It resurns the value and a bool indicating is a reset did happen.
        let whenFuncThen (pred: _ -> bool) onTrue (inputGen: LoopGen<_,_>) =
            fun state ->
                match Gen.run inputGen state with
                | Res.Continue (values, LoopState s) ->
                    let valuesUntilPred = values |> List.takeWhile (fun v -> not (pred v))
                    match valuesUntilPred.Length = values.Length with
                    | true -> Res.Continue (values, LoopState s)
                    | false -> onTrue inputGen valuesUntilPred s
                | Res.Stop values -> Res.Stop values
            |> Gen.createGen

        /// Evluates the input gen and passes it's output to the predicate function:
        /// When that returns true, the input gen is evaluated once again with an empty state.
        let whenFuncThenReset (pred: _ -> bool) (inputGen: LoopGen<_,_>) =
            whenFuncThen pred resetFuncForMapValue inputGen

        let whenFuncThenStop (pred: _ -> bool) (inputGen: LoopGen<_,_>) =
            whenFuncThen pred stopFuncForMapValue inputGen

        /// When the given predicate is true, the input gen is evaluated with an empty state.
        let whenValueThen (pred: bool) onTrue (inputGen: LoopGen<_,_>) =
            whenFuncThen (fun _ -> pred) onTrue inputGen

        /// When the given predicate is true, the input gen is evaluated with an empty state.
        let whenValueThenReset (pred: bool) (inputGen: LoopGen<_,_>) =
            whenValueThen pred resetFuncForMapValue inputGen

        let whenValueThenStop (pred: bool) (inputGen: LoopGen<_,_>) =
            whenValueThen pred stopFuncForMapValue inputGen

        let whenLoopThen (pred: LoopGen<_,_>) onTrue (inputGen: LoopGen<_,_>) =
            pred |> Gen.bind (fun pred -> whenValueThen pred onTrue inputGen)

        let whenLoopThenReset (pred: LoopGen<_,_>) (inputGen: LoopGen<_,_>) =
            whenLoopThen pred resetFuncForMapValue inputGen

        let whenLoopThenStop (pred: LoopGen<_,_>) (inputGen: LoopGen<_,_>) =
            whenLoopThen pred stopFuncForMapValue inputGen

        // TODO: doOnStop?

        let onStopThenReset (inputGen: LoopGen<_,_>) =
            fun state ->
                match Gen.run inputGen state with
                | Res.Stop values -> Res.Continue (values, LoopState None)
                | x -> x
            |> Gen.createLoop

        let inline repeatCount inclusiveStart increment inclusiveEnd =
            countTo inclusiveStart increment inclusiveEnd |> onStopThenReset

        let onCountThen count onTrue (inputGen: LoopGen<_,_>) =
            loop {
                let! c = repeatCount 0 1 (count - 1)
                return! whenValueThen (c = count) onTrue inputGen
            }

        let onCountThenReset count (inputGen: LoopGen<_,_>) =
            onCountThen count resetFuncForMapValue inputGen

        let onCountThenStop count (inputGen: LoopGen<_,_>) =
            onCountThen count stopFuncForMapValue inputGen
    
        // TODO: Test / Docu
        let includeState (inputGen: LoopGen<_,_>) =
            map2 (fun v s -> v,s) inputGen

        //// TODO: Test / Docu
        //let originalResult inputGen =
        //    fun state ->
        //        [
        //            for res in Gen.run inputGen state do
        //                match res with
        //                | Res.Emit (LoopState (_,s)) as res ->
        //                    yield Res.Emit (LoopState (res, Some s))
        //                | Res.SkipWith (LoopSkip s) as res ->
        //                    yield Res.Emit (LoopState (res, Some s))
        //                | Res.Stop as res ->
        //                    yield Res.Emit (LoopState (res, state))
        //        ]
        //    |> Gen.createGen


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

        //// TODO: Maybe fork is so important that it could be implemenmted as an own builder
        //let fork (inputGen: Gen<_,_>) =
        //    feed {
        //        let! runningStates = Init []
        //        let inputGen = Gen.run inputGen
        //        // TODO: Performance
        //        let forkResults =
        //            runningStates @ [None]
        //            |> List.map (fun forkState -> inputGen forkState |> Res.takeUntilStop)
        //        let emits =
        //            forkResults
        //            |> List.collect (fun aggRes -> Res.emittedValues aggRes.results)
        //        let newRunningStates =
        //            forkResults
        //            |> List.filter (fun res -> not res.isStopped)
        //            |> List.map (fun res -> res.finalState)
        //        if emits.Length = 0 then
        //            return Feed.SkipWith newRunningStates // in any case, emit the new state
        //        for e in emits do
        //            // TODO: it would be really great to have an "Init" counterpart - a "Store"
        //            // or something, so that the feedback state is set ONCE and not many times redundantly
        //            // when yielding more than once
        //            yield e, newRunningStates
        //    }

        //// TODO: Test / Docu
        //let windowed windowSize currentValue =
        //    accumulateOnePart windowSize currentValue |> fork


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
    
        ///// Delays a given value by 1 cycle.
        //let delay1 value =
        //    fun state ->
        //        match state with
        //        | None -> [ Res.SkipWith (None, value) ]
        //        | Some delayed -> [ Res.Emit (delayed, value) ]
        //    |> Gen.createGen
    
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
