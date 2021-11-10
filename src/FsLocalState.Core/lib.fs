[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

[<AutoOpen>]
module Gen =
    
    // --------
    // map / apply / transformation
    // --------

    let mapValueAndState (proj: 'v -> 's -> 'o) (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            [
                for res in (Gen.unwrap inputGen) state do
                    match res with
                    | Res.Emit (LoopEmit (v,s)) -> Res.Emit (LoopEmit (proj v s, s))
                    | Res.DiscardWith (LoopDiscard s) -> Res.DiscardWith (LoopDiscard s)
                    | Res.Stop -> Res.Stop
            ]
        |> Gen.create

    let mapValue proj (inputGen: LoopGen<_,_>) =
        mapValueAndState (fun v _ -> proj v) inputGen

    let includeState (inputGen: LoopGen<_,_>) =
        mapValueAndState (fun v s -> v,s) inputGen

    let apply xGen fGen =
        loop {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return Loop.Emit result
        }

    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen


    // ----------
    // reset / stop / count / ...
    // ----------

    let private emitFuncForMapValue = fun g v s -> [ Res.Emit (LoopEmit (v, s)) ]
    let private resetFuncForMapValue = fun g v s -> (Gen.unwrap g) None
    let private stopFuncForMapValue = fun g v s -> [ Res.Stop ]

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let whenFuncThen (pred: _ -> bool) onFalse onTrue (inputGen: LoopGen<_,_>) =
        fun state ->
            [
                for res in (Gen.unwrap inputGen) state do
                    match res with
                    | Res.Emit (LoopEmit (o,s)) ->
                        match pred o with
                        | false -> yield! onFalse inputGen o s
                        | true -> yield! onTrue inputGen o s
                    | Res.DiscardWith (LoopDiscard s) -> yield Res.DiscardWith (LoopDiscard s)
                    | Res.Stop -> yield Res.Stop
            ]
        |> Gen.create

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

    let whenGenThen (pred: LoopGen<_,_>) onTrue onFalse (inputGen: LoopGen<_,_>) =
        loop {
            let! pred = pred
            return! whenValueThen pred onTrue onFalse inputGen
        }

    let whenGenThenReset (pred: LoopGen<_,_>) (inputGen: LoopGen<_,_>) =
        whenGenThen pred emitFuncForMapValue resetFuncForMapValue inputGen

    let whenGenThenStop (pred: LoopGen<_,_>) (inputGen: LoopGen<_,_>) =
        whenGenThen pred emitFuncForMapValue stopFuncForMapValue inputGen

    // TODO: doOnStop?

    // TODO: Docu: Stop means thet inputGen is *immediately* reevaluated (in this cycle; not in the next)
    let onStopThenReset (inputGen: LoopGen<_,_>) =
        let rec genFunc state =
            let g = (Gen.unwrap inputGen)
            [
                for res in g state do
                    match res with
                    | Res.Stop -> yield! genFunc None
                    | _ -> yield res
            ]
        Gen.create genFunc
        
    let inline count inclusiveStart increment =
        feed {
            let! curr = Init inclusiveStart
            return Feed.Feedback (curr, curr + increment)
        }

    let inline countUntil inclusiveStart increment inclusiveEnd =
        loop {
            let! c = count inclusiveStart increment
            match c <= inclusiveEnd with
            | true -> return Loop.Emit c
            | false -> return Loop.Stop
        }

    let inline repeatCount inclusiveStart increment inclusiveEnd =
        countUntil inclusiveStart increment inclusiveEnd |> onStopThenReset

    let onCountThen count onTrue onFalse (inputGen: LoopGen<_,_>) =
        loop {
            let! c = repeatCount 0 1 (count - 1)
            return! whenValueThen (c = count) onTrue onFalse inputGen
        }

    let onCountThenReset count (inputGen: LoopGen<_,_>) =
        onCountThen count emitFuncForMapValue resetFuncForMapValue inputGen

    let onCountThenStop count (inputGen: LoopGen<_,_>) =
        onCountThen count emitFuncForMapValue stopFuncForMapValue inputGen
    
    type DefaultOnStopState<'s> = RunInput of 's option | Default

    let inline defaultOnStop defaultValue (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            let state = state |> Option.defaultValue (RunInput None)
            match state with
            | Default ->
                [ Res.Emit (LoopEmit (defaultValue, Default)) ]
            | RunInput state ->
                [
                    let mutable isRunning = true
                    for res in (Gen.unwrap inputGen) state do
                        if isRunning then
                            match res with
                            | Res.Emit (LoopEmit (v, s)) ->
                                yield Res.Emit (LoopEmit (v, RunInput (Some s)))
                            | Res.DiscardWith (LoopDiscard s) ->
                                yield Res.DiscardWith (LoopDiscard (RunInput (Some s)))
                            | Res.Stop ->
                                isRunning <- false
                                yield Res.Emit (LoopEmit (defaultValue, Default))
                ]
        |> Gen.create

    // TODO: Test / Docu
    let originalResult inputGen =
        fun state ->
            [
                for res in (Gen.unwrap inputGen) state do
                    match res with
                    | Res.Emit (LoopEmit (_,s)) as res ->
                        yield Res.Emit (LoopEmit (res, Some s))
                    | Res.DiscardWith (LoopDiscard s) as res ->
                        yield Res.Emit (LoopEmit (res, Some s))
                    | Res.Stop as res ->
                        yield Res.Emit (LoopEmit (res, state))
            ]
        |> Gen.create


    // ----------
    // accumulate
    // ----------

    let accumulate currentValue =
        feed {
            let! elements = Init []
            // TODO: Performance
            let newElements = elements @ [currentValue]
            return Feed.Feedback (newElements, newElements)
        }

    let accumulateOnePart partLength currentValue =
        loop {
            let! c = count 0 1
            let! acc = accumulate currentValue
            if c = partLength - 1 then
                // TODO Docu: Interessant - das "Stop" bedeutet nicht, dass die ganze Sequenz beendet wird, sondern
                // es bedeutet: Wenn irgendwann diese Stelle nochmal evaluiert wird, DANN (und nicht vorher) wird gestoppt.
                return Loop.Emit acc
            else if c = partLength then
                return Loop.Stop
        }

    let accumulateManyParts count currentValue =
        accumulateOnePart count currentValue |> onStopThenReset

    let fork (inputGen: Gen<_,_>) =
        feed {
            let! runningStates = Init []
            let inputGen = Gen.unwrap inputGen
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
            return Feed.Feedback (emits, newRunningStates)
        }

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
            return Feed.Feedback (random.NextDouble(), random)
        }
    
    /// Delays a given value by 1 cycle.
    let delay1 value =
        fun state ->
            match state with
            | None -> [ Res.DiscardWith (None, value) ]
            | Some delayed -> [ Res.Emit (delayed, value) ]
        |> Gen.create
    
    /// Delays a given value by n cycle.
    let delayn n value =
        feed {
            let! initialValue = Init value
            return Feed.Feedback (initialValue, initialValue)
        }
    
    /// Positive slope.
    let inline slopePos input seed =
        feed {
            let! state = Init seed
            let res = state < input
            return Feed.Feedback(res, input)
        }
    
    /// Negative slope.
    let inline slopeNeg input seed =
        feed {
            let! state = Init seed
            let res = state < input
            return Feed.Feedback(res, input)
        }
    