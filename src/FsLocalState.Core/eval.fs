module FsLocalState.Eval

open Local

let listN x n =
    x
    |> Seq.take n
    |> Seq.toList

let getValues (s: Res<_, _> seq) = s |> Seq.map (fun x -> x.value)

let noReader = fun _ -> ()

module Effect =

    /// Converts a block into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqSV getReaderState (blockWithInput: 'inp -> Local<_, _, _>) =
        let mutable lastState: 'a option = None
        fun inputValues ->
            inputValues
            |> Seq.mapi (fun i v ->
                let block = blockWithInput v |> run
                let res = block lastState (getReaderState i)
                lastState <- Some res.state
                res)

    /// Converts a block into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqV getReaderState (blockWithInput: 'inp -> Local<_, _, _>) =
        fun inputValues -> toSeqSV getReaderState blockWithInput inputValues |> getValues

module Generator =

    /// Converts a block into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqSV getReaderState (blockWithInput: Local<_, _, _>) =
        Effect.toSeqSV getReaderState (fun () -> blockWithInput) (Seq.initInfinite (fun _ -> ())) |> getValues

    /// Converts a block into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqV getReaderState (blockWithInput: Local<_, _, _>) =
        toSeqSV getReaderState blockWithInput |> getValues

module Test =
    let evalN block =
        Generator.toSeqSV noReader block |> listN
