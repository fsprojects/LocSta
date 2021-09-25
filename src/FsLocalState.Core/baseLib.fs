﻿[<AutoOpen>]
module FsLocalState.BaseLib

module Gen =

    // Ok, this is fake :) we need a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random() =
        fun _ _ -> dotnetRandom.NextDouble(), ()
        |> Gen.createForValue

    let countFrom inclusiveStart increment =
        fun s _ ->
            let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
            let newValue = state + increment
            newValue, newValue
        |> Gen.createForValue

    let count0() = countFrom 0 1
    
    // TODO: countFloat

    let singleton value =
        fun s _ ->
            let instance = Option.defaultValue value s
            instance, instance
        |> Gen.createForValue

    let singletonWith factory =
        fun s _ ->
            let instance = Option.defaultWith factory s
            instance, instance
        |> Gen.createForValue

    // TODO: think about some control constructs
    // let ifBang condition generator defaultValue =
    //     fun state (reader: unit) ->
    //         let lastXValue, lastXState = Option.defaultValue (None, None) state
    //         let xValue, xState =
    //             if condition then
    //                 let f = generator |> Gen.run
    //                 let res = f lastXState reader
    //                 Some res.value, Some res.state
    //             else
    //                 lastXValue, lastXState
    //         let newValue = Option.defaultValue defaultValue xValue
    //         { value = newValue; state = (xValue, xState) }
    //     |> Gen
    // let ( <?> ) = ifBang
    // let ( <!> ) = ( <| )

