[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

[<AutoOpen>]
module Gen =

    // ----------
    // Control
    // ----------

    let private resetFuncForDoWhen (inputGen: Gen<_,_>) =
        fun _ -> (inputGen |> Gen.unwrap) None

    let private stopFuncForDoWhen =
        fun _ -> GenResult.Stop

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let doWhenFunc (pred: _ -> bool) f (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.unwrap inputGen) state
            match res with
            | GenResult.Emit (o,s) ->
                match pred o with
                | false -> GenResult.Emit (o,s)
                | true -> f (o,s)
            | GenResult.DiscardWith s -> GenResult.DiscardWith s
            | GenResult.Discard -> GenResult.Discard
            | GenResult.Stop -> GenResult.Stop
        |> Gen.create

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    let resetWhenFunc (pred: _ -> bool) (inputGen: Gen<_,_>) =
        doWhenFunc pred (resetFuncForDoWhen inputGen) inputGen

    let stopWhenFunc (pred: _ -> bool) (inputGen: Gen<_,_>) =
        doWhenFunc pred stopFuncForDoWhen inputGen

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let doWhenValue (pred: bool) f (inputGen: Gen<_,_>) =
        inputGen |> doWhenFunc (fun _ -> pred) f

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let resetWhenValue (pred: bool) (inputGen: Gen<_,_>) =
        doWhenValue pred (resetFuncForDoWhen inputGen) inputGen

    let stopWhenValue (pred: bool) (inputGen: Gen<_,_>) =
        doWhenValue pred stopFuncForDoWhen inputGen

    let doWhenGen (pred: Gen<_,_>) f (inputGen: Gen<_,_>) =
        gen {
            let! pred = pred
            return! doWhenValue pred f inputGen
        }

    let resetWhenGen (pred: Gen<_,_>) (inputGen: Gen<_,_>) =
        doWhenGen pred (resetFuncForDoWhen inputGen) inputGen

    let stopWhenGen (pred: Gen<_,_>) (inputGen: Gen<_,_>) =
        doWhenGen pred stopFuncForDoWhen inputGen

    // TODO: doOnStop?

    let resetOnStop (inputGen: Gen<_,_>) =
        let rec genFunc state =
            let g = (Gen.unwrap inputGen)
            let res = g state
            match res with
            | GenResult.Stop -> genFunc None
            | _ -> res
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
            | true -> return Control.EmitAndLoop c
            | false -> return Control.Stop
        }

    let inline countCyclic inclusiveStart increment inclusiveEnd =
        countUntil inclusiveStart increment inclusiveEnd |> resetOnStop

    let count01<'a> = count 0 1

    let doWhenCount count f (inputGen: Gen<_,_>) =
        gen {
            let! c = countCyclic 0 1 (count - 1)
            return! doWhenValue (c = count) f inputGen
        }

    let resetWhenCount count (inputGen: Gen<_,_>) =
        doWhenCount count (resetFuncForDoWhen inputGen) inputGen

    let stopWhenCount count (inputGen: Gen<_,_>) =
        doWhenCount count stopFuncForDoWhen inputGen


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
            let newElements = currentValue :: elements
            return Control.Feedback (newElements, newElements)
        }

    let accumulateOnePart count currentValue =
        //gen {
        //    let acc = accumulate currentValue
        //}
        accumulate currentValue |> stopWhenCount count

    let accumulateManyParts count currentValue =
        accumulate currentValue |> resetWhenCount count
