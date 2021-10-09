﻿[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

module Gen =

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random<'a> () =
        fun _ (_: 'a) -> Some (dotnetRandom.NextDouble(), ())
        |> Gen.create

    let countFrom<'a> inclusiveStart increment =
        fun s (_: 'a) ->
            let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
            let newValue = state + increment
            Some (newValue, newValue)
        |> Gen.create

    let count01<'a> = countFrom<'a> 0 1

    // TODO: countFloat

    /// Delays a given value by 1 cycle.
    let delay input seed =
        seed => fun state _ ->
            gen {
                return state, input
            }

    /// Positive slope.
    let slopeP input seed =
        seed => fun state _ ->
            gen {
                let res =
                    match state, input with
                    | false, true -> true
                    | _ -> false
                return res, input
            }

    /// Negative slope.
    let slopeN input seed =
        seed => fun state _ ->
            gen {
                let res =
                    match state, input with
                    | true, false -> true
                    | _ -> false
                return res, input
            }

    // TODO
    // let toggle seed =
    //     let f p _ =
    //         match p with
    //         | true -> {value=0.0; state=false}
    //         | false -> {value=1.0; state=true}
    //     f |> liftSeed seed |> L