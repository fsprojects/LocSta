[<AutoOpen>]
module FsLocalState.Lib

module Gen =

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random() =
        fun _ _ -> Some (dotnetRandom.NextDouble(), ())
        |> Gen.create

    let countFrom inclusiveStart increment =
        fun s _ ->
            let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
            let newValue = state + increment
            Some (newValue, newValue)
        |> Gen.create

    let count0() = countFrom 0 1
    
    // TODO: countFloat

    let singletonValue value =
        fun s _ ->
            let instance = Option.defaultValue value s
            Some (instance, instance)
        |> Gen.create

    let singletonWith factory =
        fun s _ ->
            let instance = Option.defaultWith factory s
            Some (instance, instance)
        |> Gen.create

    /// Delays a given value by 1 cycle.
    let delay seed input =
        Gen.feedback seed (fun state _ ->
            gen {
                return state, input
            }
        )
    
    /// Positive slope.
    let slopeP seed input =
        Gen.feedback seed (fun state _ ->
            gen {
                let res =
                    match state, input with
                    | false, true -> true
                    | _ -> false
                return res, input
            }
        )
    
    /// Negative slope.
    let slopeN seed input =
        Gen.feedback seed (fun state _ ->
            gen {
                let res =
                    match state, input with
                    | true, false -> true
                    | _ -> false
                return res, input
            }
        )
    
    // TODO
    // let toggle seed =
    //     let f p _ =
    //         match p with
    //         | true -> {value=0.0; state=false}
    //         | false -> {value=1.0; state=true}
    //     f |> liftSeed seed |> L
