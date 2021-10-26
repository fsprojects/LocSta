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
            return (fxRes, pred), newGroups
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
            return state, input
        }

    /// Positive slope.
    let inline slopeP input seed =
        seed => fun last -> gen {
            let res = last < input
            return res, input
        }

    /// Negative slope.
    let inline slopeN input seed =
        seed => fun last -> gen {
            let res = last < input
            return res, input
        }


    // TODO
    // let toggle seed =
    //     let f p _ =
    //         match p with
    //         | true -> {value=0.0; state=false}
    //         | false -> {value=1.0; state=true}
    //     f |> liftSeed seed |> L