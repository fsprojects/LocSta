namespace FsLocalState

module Eval =

    open Core

    let getValue (x: Res<_, _>) = x.value

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqFx getReaderValue (localWithInput: LocalInput<_, _, _, _>) =
        let mutable lastState: 'a option = None
        fun inputValues ->
            inputValues
            |> Seq.mapi (fun i v ->
                let local = localWithInput v |> run
                let res = local lastState (getReaderValue i)
                lastState <- Some res.state
                res)

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqGen getReaderValue (local: Local<_, _, _>) =
        Seq.initInfinite (fun _ -> ()) |> toSeqFx getReaderValue (fun () -> local)
