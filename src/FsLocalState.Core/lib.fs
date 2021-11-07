[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

[<AutoOpen>]
module Gen =
    
    // --------
    // map / apply / transformation
    // --------

    let mapValueAndState (proj: 'v -> 's -> 'o) (inputGen: Gen<_,_>) =
        fun state ->
            [
                for res in (Gen.unwrap inputGen) state do
                    match res with
                    | GenResult.Emit (v,s) -> GenResult.Emit (proj v s, s)
                    | GenResult.DiscardWith (_,s) -> GenResult.DiscardWith (None, s)
                    | GenResult.Stop -> GenResult.Stop
            ]
        |> Gen.create

    let mapValue proj (inputGen: Gen<_,_>) =
        mapValueAndState (fun v _ -> proj v) inputGen

    let includeState (inputGen: Gen<_,_>) =
        mapValueAndState (fun v s -> v,s) inputGen

    let apply xGen fGen =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return Control.Emit result
        }

    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen


    // ----------
    // reset / stop / count / ...
    // ----------

    let private emitFuncForMapValue = fun g v s -> [ GenResult.Emit (v, s) ]
    let private resetFuncForMapValue = fun g v s -> (Gen.unwrap g) None
    let private stopFuncForMapValue = fun g v s -> [ GenResult.Stop ]

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let whenFuncThen (pred: _ -> bool) onFalse onTrue (inputGen: Gen<_,_>) =
        fun state ->
            [
                for res in (Gen.unwrap inputGen) state do
                    match res with
                    | GenResult.Emit (o,s) ->
                        match pred o with
                        | false -> yield! onFalse inputGen o s
                        | true -> yield! onTrue inputGen o s
                    | GenResult.DiscardWith (_,s) -> yield GenResult.DiscardWith (None, s)
                    | GenResult.Stop -> yield GenResult.Stop
            ]
        |> Gen.create

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    let whenFuncThenReset (pred: _ -> bool) (inputGen: Gen<_,_>) =
        whenFuncThen pred emitFuncForMapValue resetFuncForMapValue inputGen

    let whenFuncThenStop (pred: _ -> bool) (inputGen: Gen<_,_>) =
        whenFuncThen pred emitFuncForMapValue stopFuncForMapValue inputGen

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let whenValueThen (pred: bool) onTrue onFalse (inputGen: Gen<_,_>) =
        whenFuncThen (fun _ -> pred) onTrue onFalse inputGen

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let whenValueThenReset (pred: bool) (inputGen: Gen<_,_>) =
        whenValueThen pred emitFuncForMapValue resetFuncForMapValue inputGen

    let whenValueThenStop (pred: bool) (inputGen: Gen<_,_>) =
        whenValueThen pred emitFuncForMapValue stopFuncForMapValue inputGen

    let whenGenThen (pred: Gen<_,_>) onTrue onFalse (inputGen: Gen<_,_>) =
        gen {
            let! pred = pred
            return! whenValueThen pred onTrue onFalse inputGen
        }

    let whenGenThenReset (pred: Gen<_,_>) (inputGen: Gen<_,_>) =
        whenGenThen pred emitFuncForMapValue resetFuncForMapValue inputGen

    let whenGenThenStop (pred: Gen<_,_>) (inputGen: Gen<_,_>) =
        whenGenThen pred emitFuncForMapValue stopFuncForMapValue inputGen

    // TODO: doOnStop?

    // TODO: Docu: Stop means thet inputGen is *immediately* reevaluated (in this cycle; not in the next)
    let onStopThenReset (inputGen: Gen<_,_>) =
        let rec genFunc state =
            let g = (Gen.unwrap inputGen)
            [
                for res in g state do
                    match res with
                    | GenResult.Stop -> yield! genFunc None
                    | _ -> yield res
            ]
        Gen.create genFunc
        
    let inline count inclusiveStart increment =
        fdb {
            let! curr = Init inclusiveStart
            return Control.Feedback (curr, curr + increment)
        }

    let inline countUntil inclusiveStart increment inclusiveEnd =
        gen {
            let! c = count inclusiveStart increment
            match c <= inclusiveEnd with
            | true -> return Control.Emit c
            | false -> return Control.Stop
        }

    let inline repeatCount inclusiveStart increment inclusiveEnd =
        countUntil inclusiveStart increment inclusiveEnd |> onStopThenReset

    let onCountThen count onTrue onFalse (inputGen: Gen<_,_>) =
        gen {
            let! c = repeatCount 0 1 (count - 1)
            return! whenValueThen (c = count) onTrue onFalse inputGen
        }

    let onCountThenReset count (inputGen: Gen<_,_>) =
        onCountThen count emitFuncForMapValue resetFuncForMapValue inputGen

    let onCountThenStop count (inputGen: Gen<_,_>) =
        onCountThen count emitFuncForMapValue stopFuncForMapValue inputGen
    
    type DefaultOnStopState<'s> = RunInput of 's option | Default

    let inline defaultOnStop defaultValue (inputGen: Gen<_,_>) =
        fun state ->
            let state = state |> Option.defaultValue (RunInput None)
            match state with
            | Default ->
                [ GenResult.Emit (defaultValue, Default) ]
            | RunInput state ->
                [
                    let mutable isRunning = true
                    for res in (Gen.unwrap inputGen) state do
                        if isRunning then
                            match res with
                            | GenResult.Emit (v, s) ->
                                yield GenResult.Emit (v, RunInput (Some s))
                            | GenResult.DiscardWith (_,s) ->
                                yield GenResult.DiscardWith (None, RunInput (Some s))
                            | GenResult.Stop ->
                                isRunning <- false
                                yield GenResult.Emit (defaultValue, Default)
                ]
        |> Gen.create

    // TODO: Test / Docu
    let originalResult inputGen =
        fun state ->
            [
                for res in (Gen.unwrap inputGen) state do
                    match res with
                    | GenResult.Emit (v,s) as res ->
                        yield GenResult.Emit (res, s)
                    | GenResult.DiscardWith (_,s) as res ->
                        yield GenResult.Emit (res, s)
                    | GenResult.Stop as res ->
                        yield GenResult.Emit (res, state)
            ]
        |> Gen.create


    // ----------
    // accumulate
    // ----------

    let accumulate currentValue =
        fdb {
            let! elements = Init []
            // TODO: Performance
            let newElements = elements @ [currentValue]
            return Control.Feedback (newElements, newElements)
        }

    let accumulateOnePart partLength currentValue =
        gen {
            let! c = count 0 1
            let! acc = accumulate currentValue
            if c = partLength - 1 then
                // TODO Docu: Interessant - das "Stop" bedeutet nicht, dass die ganze Sequenz beendet wird, sondern
                // es bedeutet: Wenn irgendwann diese Stelle nochmal evaluiert wird, DANN (und nicht vorher) wird gestoppt.
                return Control.Emit acc
            else if c = partLength then
                return Control.Stop
        }

    let accumulateManyParts count currentValue =
        accumulateOnePart count currentValue |> onStopThenReset

    let fork (inputGen: Gen<_,_>) =
        fdb {
            let! runningStates = Init []
            let inputGen = Gen.unwrap inputGen
            // TODO: Performance
            let forkResults =
                runningStates @ [None]
                |> List.map (fun forkState -> inputGen forkState |> GenResult.takeUntilStop)
            let emits =
                forkResults
                |> List.collect (fun aggRes -> GenResult.emittedValues aggRes.results)
            let newRunningStates =
                forkResults
                |> List.filter (fun res -> not res.isStopped)
                |> List.map (fun res -> res.finalState)
            return Control.Feedback (emits, newRunningStates)
        }

    let windowed windowSize currentValue =
        accumulateOnePart windowSize currentValue |> fork


    // ----------
    // random / delay / slope
    // ----------
    
    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom() = System.Random()
    let random () =
        fdb {
            let! random = Init (dotnetRandom())
            return Control.Feedback (random.NextDouble(), random)
        }
    
    /// Delays a given value by 1 cycle.
    let delay1 value =
        fun state ->
            match state with
            | None -> [ GenResult.DiscardWith (None, value) ]
            | Some delayed -> [ GenResult.Emit (delayed, value) ]
        |> Gen.create
    
    /// Delays a given value by n cycle.
    let delayn n value =
        fdb {
            let! initialValue = Init value
            return Control.Feedback (initialValue, initialValue)
        }
    
    /// Positive slope.
    let inline slopePos input seed =
        fdb {
            let! state = Init seed
            let res = state < input
            return Control.Feedback(res, input)
        }
    
    /// Negative slope.
    let inline slopeNeg input seed =
        fdb {
            let! state = Init seed
            let res = state < input
            return Control.Feedback(res, input)
        }
    