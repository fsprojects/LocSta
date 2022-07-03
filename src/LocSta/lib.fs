namespace LocSta

// TODO: document each one of those
[<AutoOpen>]
module Lib =

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
