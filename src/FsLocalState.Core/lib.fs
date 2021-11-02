[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

[<AutoOpen>]
module Gen =

    // ----------
    // Control
    // ----------

    let mapValueAndState (proj: 'v -> 's -> 'a) (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.unwrap inputGen) state
            match res with
            | GenResult.ValueAndState (v,s) -> GenResult.ValueAndState (proj v s, s)
            | GenResult.DiscardWith s -> GenResult.DiscardWith s
            | GenResult.Discard -> GenResult.Discard
            | GenResult.Stop -> GenResult.Stop
        |> Gen.create

    let mapValue proj (inputGen: Gen<_,_>) =
        mapValueAndState (fun v _ -> proj v) inputGen

    let includeState (inputGen: Gen<_,_>) =
        mapValueAndState (fun v s -> v,s) inputGen

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let resetWhenFunc2 (pred: _ -> bool) (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.unwrap inputGen) state
            match res with
            | GenResult.ValueAndState (o,s) ->
                match pred o with
                | false -> GenResult.ValueAndState ((o,false), s)
                | true -> (inputGen |> mapValue (fun v -> v,true) |> Gen.unwrap) None
            | GenResult.DiscardWith s -> GenResult.DiscardWith s
            | GenResult.Discard -> GenResult.Discard
            | GenResult.Stop -> GenResult.Stop
        |> Gen.create

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    let resetWhenFunc (pred: _ -> bool) (inputGen: Gen<_,_>) =
        inputGen |> resetWhenFunc2 pred |> mapValue fst

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let resetWhenValue (pred: bool) (inputGen: Gen<_,_>) =
        inputGen |> resetWhenFunc (fun _ -> pred)

    let resetWhenGen (pred: Gen<_,_>) (inputGen: Gen<_,_>) =
        gen {
            let! pred = pred
            return! resetWhenValue pred inputGen
        }
    
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
            return Res.Feedback (curr, curr + increment)
        }

    let inline countUntil inclusiveStart increment inclusiveEnd =
        gen {
            let! c = count inclusiveStart increment
            match c <= inclusiveEnd with
            | true -> return Res.ValueAndLoop c
            | false -> return Res.Stop
        }

    let inline countCyclic inclusiveStart increment inclusiveEnd =
        countUntil inclusiveStart increment inclusiveEnd |> resetOnStop

    let count01<'a> = count 0 1

    let resetWhenCount count (inputGen: Gen<_,_>) =
        gen {
            let! c = countCyclic 0 1 (count - 1)
            return! resetWhenValue (c = count) inputGen
        }

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom() = System.Random()
    let random () =
        fdb {
            let! random = Init (dotnetRandom())
            return Res.Feedback (random.NextDouble(), random)
        }

    /// Delays a given value by 1 cycle.
    let delayBy1 input seed =
        fdb {
            let! state = Init seed
            return Res.Feedback(state, input)
        }

    /// Positive slope.
    let inline slopePos input seed =
        fdb {
            let! state = Init seed
            let res = state < input
            return Res.Feedback(res, input)
        }

    /// Negative slope.
    let inline slopeNeg input seed =
        fdb {
            let! state = Init seed
            let res = state < input
            return Res.Feedback(res, input)
        }

    let accumulate currentValue =
        fdb {
            let! elements = Init []
            let newElements = currentValue :: elements
            return Res.Feedback (newElements, newElements)
        }

    let accumulateOnce count currentValue =
        accumulate currentValue |> resetWhenCount count

    let accumulateAll count currentValue =
        accumulateOnce count currentValue |> resetOnStop
