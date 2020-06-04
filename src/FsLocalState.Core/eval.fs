module FsLocalState.Eval

open Core

let noReader = ignore

let getValue (x: Res<_, _>) = x.value

/// Converts a local into a sequence with the given state.
/// The getReaderState function is called for each evaluation.
let toSeqi getReaderValue (localWithInput: LocalInput<_, _, _, _>) =
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
let toSeq getReaderValue (localWithInput: LocalInput<_, _, _, _>) =
    toSeqi (fun _ -> getReaderValue ()) localWithInput

/// Converts a local into a sequence with the given state.
/// The getReaderState function is called for each evaluation.
let toSeqValuesi getReaderValue (localWithInput: LocalInput<_, _, _, _>) =
    fun inputValues ->
        toSeqi getReaderValue localWithInput inputValues
        |> Seq.map getValue

/// Converts a local into a sequence with the given state.
/// The getReaderState function is called for each evaluation.
let toSeqValues getReaderValue (localWithInput: LocalInput<_, _, _, _>) =
    fun inputValues ->
        toSeq getReaderValue localWithInput inputValues
        |> Seq.map getValue

/// Converts a local into a sequence with the given state.
/// The getReaderState function is called for each evaluation.
let toSeqGen2 getReaderValue (local: Local<_, _, _>) =
    Seq.initInfinite (fun _ -> ()) |> toSeq getReaderValue (fun () -> local)

/// Converts a local into a sequence with the given state.
/// The getReaderState function is called for each evaluation.
let toSeqGen getReaderValue (local: Local<_, _, _>) =
    toSeqGen2 getReaderValue local |> Seq.map getValue

let pulln n seq =
    seq
    |> Seq.take n
    |> Seq.toList
