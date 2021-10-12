[<AutoOpen>]
module FsLocalState.Eval

open FsLocalState
    
module Gen =

    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStart state (g: Gen<_,_>) =
        let f = Gen.run g
        let mutable state = state
        seq {
            while true do
                match f state with
                | Some res ->
                    state <- Some (snd res)
                    yield res
                | None -> ()
        }
    
    let resume state (g: Gen<_,_>) = resumeOrStart (Some state) g

    let toSeqStateFx (fx: Fx<_,_,_>) =
        let mutable lastState = None
        fun inputValues ->
            seq {
                for i,v in inputValues |> Seq.indexed do
                    let local = fx v |> Gen.run
                    match local lastState with
                    | Some res ->
                        lastState <- Some (snd res)
                        yield res
                    | None -> ()
            }

    let toSeqFx  (fx: Fx<_,_,_>) =
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
