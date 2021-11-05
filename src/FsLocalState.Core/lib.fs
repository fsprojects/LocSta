[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

[<AutoOpen>]
module Gen =

    // ----------
    // Control
    // ----------

    let private emitFuncForDoWhen = fun g v s -> [ GenResult.Emit (v, s) ]
    let private resetFuncForDoWhen = fun g v s -> (Gen.unwrap g) None
    let private stopFuncForDoWhen = fun g v s -> [ GenResult.Stop ]

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let doWhenFunc (pred: _ -> bool) onFalse onTrue (inputGen: Gen<_,_>) =
        fun state ->
            [
                for res in (Gen.unwrap inputGen) state do
                    match res with
                    | GenResult.Emit (o,s) ->
                        match pred o with
                        | false -> yield! onFalse inputGen o s
                        | true -> yield! onTrue inputGen o s
                    | GenResult.DiscardWith s -> yield GenResult.DiscardWith s
                    | GenResult.Stop -> yield GenResult.Stop
            ]
        |> Gen.create

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    let resetWhenFunc (pred: _ -> bool) (inputGen: Gen<_,_>) =
        doWhenFunc pred emitFuncForDoWhen resetFuncForDoWhen inputGen

    let stopWhenFunc (pred: _ -> bool) (inputGen: Gen<_,_>) =
        doWhenFunc pred emitFuncForDoWhen stopFuncForDoWhen inputGen

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let doWhenValue (pred: bool) onTrue onFalse (inputGen: Gen<_,_>) =
        doWhenFunc (fun _ -> pred) onTrue onFalse inputGen

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let resetWhenValue (pred: bool) (inputGen: Gen<_,_>) =
        doWhenValue pred emitFuncForDoWhen resetFuncForDoWhen inputGen

    let stopWhenValue (pred: bool) (inputGen: Gen<_,_>) =
        doWhenValue pred emitFuncForDoWhen stopFuncForDoWhen inputGen

    let doWhenGen (pred: Gen<_,_>) onTrue onFalse (inputGen: Gen<_,_>) =
        gen {
            let! pred = pred
            return! doWhenValue pred onTrue onFalse inputGen
        }

    let resetWhenGen (pred: Gen<_,_>) (inputGen: Gen<_,_>) =
        doWhenGen pred emitFuncForDoWhen resetFuncForDoWhen inputGen

    let stopWhenGen (pred: Gen<_,_>) (inputGen: Gen<_,_>) =
        doWhenGen pred emitFuncForDoWhen stopFuncForDoWhen inputGen

    // TODO: doOnStop?

    // TODO: Docu: Stop means thet inputGen is *immediately* reevaluated (in this cycle; not in the next)
    let resetOnStop (inputGen: Gen<_,_>) =
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

    let inline countCyclic inclusiveStart increment inclusiveEnd =
        countUntil inclusiveStart increment inclusiveEnd |> resetOnStop

    let count01<'a> = count 0 1

    let doWhenCount count onTrue onFalse (inputGen: Gen<_,_>) =
        gen {
            let! c = countCyclic 0 1 (count - 1)
            return! doWhenValue (c = count) onTrue onFalse inputGen
        }

    let resetWhenCount count (inputGen: Gen<_,_>) =
        doWhenCount count emitFuncForDoWhen resetFuncForDoWhen inputGen

    let stopWhenCount count (inputGen: Gen<_,_>) =
        doWhenCount count emitFuncForDoWhen stopFuncForDoWhen inputGen


    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom() = System.Random()
    let random () =
        fdb {
            let! random = Init (dotnetRandom())
            return Control.Feedback (random.NextDouble(), random)
        }

    /// Delays a given value by 1 cycle.
    let delayBy1 input seed =
        fdb {
            let! state = Init seed
            return Control.Feedback(state, input)
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

    let accumulate currentValue =
        fdb {
            let! elements = Init []
            // TODO: Performance
            let newElements = elements @ [currentValue]
            return Control.Feedback (newElements, newElements)
        }

    let accumulateOnePart count currentValue =
        gen {
            let! c = count01
            let! acc = accumulate currentValue
            if c = count - 1 then
                // TODO Docu: Interessant - das "Stop" bedeutet nicht, dass die ganze Sequenz beendet wird, sondern
                // es bedeutet: Wenn irgendwann diese Stelle nochmal evaluiert wird, DANN (und nicht vorher) wird gestoppt.
                return Control.Emit acc
            else if c = count then
                return Control.Stop
        }

    let accumulateManyParts count currentValue =
        accumulateOnePart count currentValue |> resetOnStop
