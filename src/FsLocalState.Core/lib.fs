[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

[<AutoOpen>]
module Gen =

    // ----------
    // Control
    // ----------

    let mapValue2 proj (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.unwrap inputGen) state
            match res with
            | ValueAndState (v,s) -> ValueAndState (proj v s, s)
            | DiscardWith s -> DiscardWith s
            | Discard -> Discard
            | Stop -> Stop
        |> Gen.create

    let mapValue proj (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.unwrap inputGen) state
            match res with
            | ValueAndState (v,s) -> ValueAndState (proj v, s)
            | DiscardWith s -> DiscardWith s
            | Discard -> Discard
            | Stop -> Stop
        |> Gen.create

    let includeState (inputGen: Gen<_,_>) =
        inputGen |> mapValue2 (fun v s -> v,s)

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let resetWithCurrent2 (pred: _ -> bool) (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.unwrap inputGen) state
            match res with
            | ValueAndState (o,s) ->
                match pred o with
                | false -> ValueAndState ((o,false), s)
                | true -> (inputGen |> mapValue (fun v -> v,true) |> Gen.unwrap) None
            | DiscardWith s -> DiscardWith s
            | Discard -> Discard
            | Stop -> Stop
        |> Gen.create

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    let resetWithCurrent (pred: _ -> bool) (inputGen: Gen<_,_>) =
        inputGen |> resetWithCurrent2 pred |> mapValue fst

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let reset (pred: bool) (inputGen: Gen<_,_>) =
        inputGen |> resetWithCurrent (fun _ -> pred)
        
    //let partitionMapWithCurrent2 (pred: 'o -> bool) (proj: Fx<_,_,_>) (inputGen: Gen<'o,_>) =
    //    [] => fun groups -> gen {
    //        let! res = inputGen
    //        let pred = pred res
    //        let newGroups =
    //            match pred with
    //            | true -> [res] :: groups
    //            | false ->
    //                match groups with
    //                | [] -> [ [res] ]
    //                | x::xs -> [ res :: x; yield! xs ]
    //        let! fxRes = proj res
    //        return Res.feedback (fxRes, pred) newGroups
    //    }
        
    //let partitionWithCurrent2 (pred: 'o -> bool) (inputGen: Gen<'o,_>) =
    //    partitionMapWithCurrent2 pred Gen.ofValue inputGen

    //let partitionMapWithCurrent (pred: 'o -> bool) (fx: Fx<_,_,_>) (inputGen: Gen<'o,_>) =
    //    inputGen |> partitionMapWithCurrent2 pred fx |> mapValue fst

    //let partitionWithCurrent (pred: 'o -> bool) (inputGen: Gen<'o,_>) =
    //    partitionMapWithCurrent pred Gen.ofValue inputGen

    //let partitionMap (pred: bool) (fx: Fx<_,_,_>) (inputGen: Gen<'o,_>) =
    //    partitionMapWithCurrent (fun _ -> pred) fx inputGen

    //let partition (pred: bool) (inputGen: Gen<'o,_>) =
    //    partitionMap pred Gen.ofValue inputGen

    //type FilterMapState<'v,'s> = { startValue: 'v; state: 's option }

    //let filterMapFx (pred: 'i -> Fx<'i,'o,'s2>) (inputGen: Gen<'i,'s1>) =
    //    [] => fun currentChecks -> gen {
    //        let! currentValue = inputGen
    //        let checks =
    //            currentChecks
    //            |> List.map (fun checkState ->
    //                let checkRes = (pred checkState.startValue currentValue |> Gen.asFunc) checkState.state
    //                match checkRes with
    //                | Value (res,_) -> Choice1Of3 (checkState.startValue, res)
    //                | Discard s ->
    //                    match s with
    //                    | Some s -> Some s
    //                    | None -> checkState.state
    //                    |> fun s -> Choice2Of3 { checkState with state = s }
    //                | Stop -> Choice3Of3 ()
    //            )
    //        let successChecks = checks |> List.choose (function | Choice1Of3 v -> Some v | _ -> None)
    //        let activeChecks = checks |> List.choose (function | Choice2Of3 s -> Some s | _ -> None)

    //        failwith "TODO"
    //        //let ongoingChecks = { startValue = currentValue; state = None } :: activeChecks
    //        //if successChecks.Length > 0
    //        //    then yield Value (successChecks, { mine = ongoingChecks, ())
    //        //    else yield ContinueWithState ongoingChecks
    //    }

    //let filterMap (pred: 'i -> 'i -> GenResult<'i,_>) (inputGen: Gen<'i,'s>) =
    //    inputGen |> filterMapFx (fun start curr -> Gen.ofValue (pred start curr))

    //// TODO: Implement a random number generator that exposes it's serializable state.
    //let private dotnetRandom = System.Random()
    //let random () =
    //    fdb {
    //        let! random = init dotnetRandom
    //        return Res.feedback (dotnetRandom.NextDouble()) random
    //    }

    let inline count inclusiveStart increment =
        fdb {
            let! curr = Init inclusiveStart
            return Res.feedback curr (curr + increment)
        }

    let count01<'a> = count 0 1

    // TODO: countFloat

    /// Delays a given value by 1 cycle.
    let delay input seed =
        fdb {
            let! state = Init seed
            return Res.feedback state input
        }

    /// Positive slope.
    let inline slopeP input seed =
        fdb {
            let! state = Init seed
            let res = state < input
            return Res.feedback res input
        }

    /// Negative slope.
    let inline slopeN input seed =
        fdb {
            let! state = Init seed
            let res = state < input
            return Res.feedback res input
        }
