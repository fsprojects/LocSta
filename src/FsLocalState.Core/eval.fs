﻿namespace FsLocalState

open FsLocalState.Core

module Eval =
    
    let getValues s = s |> List.map Geneff.getValue

    let toEvaluable getReaderValue (localWithInput: Eff<_, _, _, _>) =
        let mutable lastState: 'a option = None
        fun inputValues ->
            inputValues
            |> Seq.mapi (fun i v ->
                let local = localWithInput v |> run
                let res = local lastState (getReaderValue i)
                lastState <- Some res.state
                res)
            |> Seq.toList

    let toEvaluableValues getReaderValue (localWithInput: Eff<_, _, _, _>) =
        let evaluable = toEvaluable getReaderValue localWithInput
        fun inputValues -> evaluable inputValues |> getValues
    //
    //
    // module Fx =
    //
    //     let toEvaluable getReaderValue (localWithInput: Eff<_, _, _, _>) =
    //         let mutable lastState: 'a option = None
    //         fun inputValues ->
    //             inputValues
    //             |> Seq.mapi (fun i v ->
    //                 let local = localWithInput v |> run
    //                 let res = local lastState (getReaderValue i)
    //                 lastState <- Some res.state
    //                 res)
    //             |> Seq.toList
    //
    //     let toEvaluableValues getReaderValue (localWithInput: Eff<_, _, _, _>) =
    //         let evaluable = toEvaluable getReaderValue localWithInput
    //         fun inputValues -> evaluable inputValues |> getValues
    //         
    //
    // module Gen =
    //
    //     let toEvaluable getReaderValue (local: Gen<_, _, _>) =
    //         
    //         // first. transform the gen to an effect
    //         let fx : Eff<_, _, _, _> =
    //             fun () -> local
    //
    //         let evaluable = Fx.toEvaluable getReaderValue fx
    //         
    //         // now, we don't want to have a "seq<_> -> list<Res<_,_>>", but an "int -> list<Res<_,_>>"
    //         fun n ->
    //             let inputSeq = Seq.init n ignore
    //             let resultingValues = evaluable inputSeq
    //             resultingValues
    //
    //     let toEvaluableValues getReaderValue (local: Gen<_, _, _>) =
    //         let evaluable = toEvaluable getReaderValue local
    //         fun n -> evaluable n |> getValues
