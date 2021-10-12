﻿[<AutoOpen>]
module FsLocalState.Lib

open FsLocalState

module Gen =

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random () =
        fun _ -> Some (dotnetRandom.NextDouble(), ())
        |> Gen.create

    let count inclusiveStart increment =
        fun s ->
            let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
            let newValue = state + increment
            Some (newValue, newValue)
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