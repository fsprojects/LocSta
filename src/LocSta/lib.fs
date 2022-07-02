namespace LocSta

// TODO: document each one of those
[<AutoOpen>]
module Lib =


    // -------
    // count
    // -------

    let inline count inclusiveStart increment =
        feed {
            let! curr = Init inclusiveStart
            yield curr, curr + increment
        }

    let inline countToAndThen inclusiveStart increment inclusiveEnd onEnd =
        loop {
            let! c = count inclusiveStart increment
            match c <= inclusiveEnd with
            | true -> yield c
            | false -> return! (Gen.createLoop (fun _ -> onEnd))
        }

    let inline countTo inclusiveStart increment inclusiveEnd =
        countToAndThen inclusiveStart increment inclusiveEnd Res.Loop.stop

    let inline countToCyclic inclusiveStart increment inclusiveEnd =
        countToAndThen inclusiveStart increment inclusiveEnd Res.Loop.skipAndReset


    // ----------
    // reset / stop
    // ----------

    let private resetFuncForMapValue = fun gen _ _ -> Gen.run gen None
    let private stopFuncForMapValue = fun _ values _ -> Res.Stop values

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let whenFuncThen (pred: _ -> bool) onTrue (inputGen: LoopGen<_,_>) =
        fun state ->
            match Gen.run inputGen state with
            | Res.Continue (values, state) ->
                let valuesUntilPred = values |> List.takeWhile (fun v -> not (pred v))
                match valuesUntilPred.Length = values.Length with
                | true -> Res.Continue (values, state)
                | false -> onTrue inputGen valuesUntilPred state
            | Res.Stop values -> Res.Stop values
        |> Gen.createLoop

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
            | Res.Stop values -> Res.Loop.emitManyAndReset values
            | x -> x
        |> Gen.createLoop

    let onCountThen count onTrue (inputGen: LoopGen<_,_>) =
        loop {
            let! c = countToCyclic 0 1 (count - 1)
            return! whenValueThen (c = count) onTrue inputGen
        }

    let onCountThenReset count (inputGen: LoopGen<_,_>) =
        onCountThen count resetFuncForMapValue inputGen

    let onCountThenStop count (inputGen: LoopGen<_,_>) =
        onCountThen count stopFuncForMapValue inputGen
    
    // TODO: Test / Docu
    let includeState (inputGen: LoopGen<_,_>) =
        Gen.mapValueAndState (fun v s -> v,s) inputGen

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
    // accumulate
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

        
    // ----------
    // accumulate
    // ----------

    let fork (inputGen: Gen<_,_>) =
        feed {
            let! runningStates = Init []
            let inputGen = Gen.run inputGen
            // TODO: Performance / unclear code
            let mutable resultValues = []
            let newForkStates = [
                for forkState in None :: runningStates do
                    match inputGen forkState with
                    | Res.Continue (values, s) ->
                        resultValues <- resultValues @ values
                        yield Some s
                    | Res.Stop values ->
                        resultValues <- resultValues @ values
                        yield None
                ]
            return Feed.EmitMany (resultValues, newForkStates)
        }

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
    //    |> Gen.createLoop
    
    ///// Delays a given value by n cycle.
    //let delayn n value =
    //    feed {
    //        let! initialValue = Init value
    //        return Feed.Feedback (initialValue, initialValue)
    //    }

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
            let! i = count 0 1
            let! value = inputGen
            if i >= s then
                match inclEndIdx with
                | Some e when i > e ->
                    return Loop.Stop
                | _ ->
                    yield value
        }
