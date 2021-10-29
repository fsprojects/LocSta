﻿[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

[<AutoOpen>]
module Gen =

    // ----------
    // Control
    // ----------

    let mapValue2 proj (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.run inputGen) state
            match res with
            | Value (v,s) -> Value(proj v s, s)
            | Discard s -> Discard s
            | Stop -> Stop
        |> Gen.create

    let mapValue proj (inputGen: Gen<_,_>) =
        fun state ->
            let res = (Gen.run inputGen) state
            match res with
            | Value (v,s) -> Value(proj v, s)
            | Discard s -> Discard s
            | Stop -> Stop
        |> Gen.create

    let includeState (inputGen: Gen<_,_>) =
        inputGen |> mapValue2 (fun v s -> v,s)

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    /// It resurns the value and a bool indicating is a reset did happen.
    let resetWithCurrent2 (pred: 'o -> bool) (inputGen: Gen<'o,_>) =
        fun state ->
            let res = (Gen.run inputGen) state
            match res with
            | Value (o,s) ->
                match pred o with
                | false -> Value((o,false), s)
                | true -> (inputGen |> mapValue (fun v -> v,true) |> Gen.run) None
            | Discard s -> Discard s
            | Stop -> Stop
        |> Gen.create

    /// Evluates the input gen and passes it's output to the predicate function:
    /// When that returns true, the input gen is evaluated once again with an empty state.
    let resetWithCurrent (pred: 'o -> bool) (inputGen: Gen<'o,_>) =
        inputGen |> resetWithCurrent2 pred |> mapValue fst

    /// When the given predicate is true, the input gen is evaluated with an empty state.
    let reset (pred: bool) (inputGen: Gen<_,_>) =
        inputGen |> resetWithCurrent (fun _ -> pred)
        
    let partitionMapWithCurrent2 (pred: 'o -> bool) (proj: Fx<_,_,_>) (inputGen: Gen<'o,_>) =
        [] => fun groups -> gen {
            let! res = inputGen
            let pred = pred res
            let newGroups =
                match pred with
                | true -> [res] :: groups
                | false ->
                    match groups with
                    | [] -> [ [res] ]
                    | x::xs -> [ res :: x; yield! xs ]
            let! fxRes = proj res
            return Value (((fxRes, pred), newGroups), ())
        }
        
    let partitionWithCurrent2 (pred: 'o -> bool) (inputGen: Gen<'o,_>) =
        partitionMapWithCurrent2 pred Gen.ofValue inputGen

    let partitionMapWithCurrent (pred: 'o -> bool) (fx: Fx<_,_,_>) (inputGen: Gen<'o,_>) =
        inputGen |> partitionMapWithCurrent2 pred fx |> mapValue fst

    let partitionWithCurrent (pred: 'o -> bool) (inputGen: Gen<'o,_>) =
        partitionMapWithCurrent pred Gen.ofValue inputGen

    let partitionMap (pred: bool) (fx: Fx<_,_,_>) (inputGen: Gen<'o,_>) =
        partitionMapWithCurrent (fun _ -> pred) fx inputGen

    let partition (pred: bool) (inputGen: Gen<'o,_>) =
        partitionMap pred Gen.ofValue inputGen

    type FilterMapState<'v,'s> = { startValue: 'v; state: 's option }

    let filterMapFx (pred: 'i -> Fx<'i,'o,'s2>) (inputGen: Gen<'i,'s1>) =
        [] => fun currentChecks -> gen {
            let! currentValue = inputGen
            let checks =
                currentChecks
                |> List.map (fun checkState ->
                    let checkRes = (pred checkState.startValue currentValue |> Gen.run) checkState.state
                    match checkRes with
                    | Value (res,_) -> Choice1Of3 (checkState.startValue, res)
                    | Discard s ->
                        match s with
                        | Some s -> Some s
                        | None -> checkState.state
                        |> fun s -> Choice2Of3 { checkState with state = s }
                    | Stop -> Choice3Of3 ()
                )
            let successChecks = checks |> List.choose (function | Choice1Of3 v -> Some v | _ -> None)
            let activeChecks = checks |> List.choose (function | Choice2Of3 s -> Some s | _ -> None)

            failwith "TODO"
            //let ongoingChecks = { startValue = currentValue; state = None } :: activeChecks
            //if successChecks.Length > 0
            //    then yield Value (successChecks, { mine = ongoingChecks, ())
            //    else yield ContinueWithState ongoingChecks
        }

    let filterMap (pred: 'i -> 'i -> GenResult<'i,_>) (inputGen: Gen<'i,'s>) =
        inputGen |> filterMapFx (fun start curr -> Gen.ofValue (pred start curr))

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random () =
        fun _ -> Value (dotnetRandom.NextDouble(), ())
        |> Gen.create

    let count inclusiveStart increment =
        fun s ->
            let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
            let newValue = state + increment
            Value (newValue, newValue)
        |> Gen.create

    let count_0_1 = count 0 1

    // TODO: countFloat

    /// Delays a given value by 1 cycle.
    let delay input seed =
        seed => fun state -> gen {
            return Value ((state, input), ())
        }

    /// Positive slope.
    let inline slopeP input seed =
        seed => fun last -> gen {
            let res = last < input
            return Value ((res, input), ())
        }

    /// Negative slope.
    let inline slopeN input seed =
        seed => fun last -> gen {
            let res = last < input
            return Value ((res, input), ())
        }


    // TODO
    // let toggle seed =
    //     let f p _ =
    //         match p with
    //         | true -> {value=0.0; state=false}
    //         | false -> {value=1.0; state=true}
    //     f |> liftSeed seed |> L