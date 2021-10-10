[<AutoOpen>]
module FsLocalState.Eval

open FsLocalState
    
module Gen =

    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStart (state: 's option) (g: Gen<'o,'s>) =
        seq {
            let f = Gen.run g
            let mutable lastState = state
            let mutable loop = true

            while loop do
                match f lastState with
                | Value (res, s) ->
                    lastState <- Some s
                    yield res
                | Discard s -> ()
                | Stop -> loop <- false
        }
    
    let resume state (g: Gen<_,_>) = resumeOrStart (Some state) g

    let toSeqStateFx (fx: Fx<_,_,_>) =
        let mutable lastState = None
        let mutable loop = true

        fun (inputValues: seq<_>) ->
            let inputEnumerator = inputValues.GetEnumerator()
            seq {
                while inputEnumerator.MoveNext() && loop do
                    let i = inputEnumerator.Current
                    let local = fx i |> Gen.run
                    match local lastState with
                    | Value (res, s) ->
                        lastState <- Some s
                        yield res
                    | Discard s -> ()
                    | Stop -> loop <- false
            }

    let toSeqFx (fx: Fx<_,_,_>) =
        let evaluable = toSeqStateFx fx
        fun inputValues -> evaluable inputValues |> Seq.map fst
    
    let toSeqState (g: Gen<_,_>) = resumeOrStart None g
    
    let toSeq (g: Gen<_,_>) = toSeqState g |> Seq.map fst

    let toListFx fx inputSeq =
        inputSeq |> toSeqFx fx |> Seq.toList
    
    let toList gen =
        toSeq gen |> Seq.toList
    
    let toListn count gen =
        toSeq gen |> Seq.truncate count |> Seq.toList
