[<AutoOpen>]
module FsLocalState.Eval

open FsLocalState
    
module Gen =

    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStartReader getReaderValue state (g: Gen<_,_,_>) =
        let f = Gen.run g
        let mutable state = state
        seq {
            while true do
                match f state (getReaderValue()) with
                | Some res ->
                    state <- Some (snd res)
                    yield res
                | None -> ()
        }
       
    let resumeOrStart state (g: Gen<_,_,_>) = resumeOrStartReader ignore state g
    
    let resumeReader getReaderValue state (g: Gen<_,_,_>) =
        resumeOrStartReader getReaderValue (Some state) g

    let resume state (g: Gen<_,_,_>) = resumeReader ignore state g

    let toSeqStateReaderFx (getReaderValue: int -> 'r) (fx: Fx<_,_,_,'r>) =
        let mutable lastState = None
        fun inputValues ->
            seq {
                for i,v in inputValues |> Seq.indexed do
                    let local = fx v |> Gen.run
                    match local lastState (getReaderValue i) with
                    | Some res ->
                        lastState <- Some (snd res)
                        yield res
                    | None -> ()
            }

    let toSeqStateFx (fx: Fx<_,_,_,_>) = toSeqStateReaderFx ignore fx

    let toSeqReaderFx  (getReaderValue: int -> 'r) (fx: Fx<_,_,_,'r>) =
        let evaluable = toSeqStateReaderFx getReaderValue fx
        fun inputValues -> evaluable inputValues |> Seq.map fst
    
    let toSeqFx (fx: Fx<_,_,_,_>) = toSeqReaderFx ignore fx
    
    let toSeqStateReader getReaderValue (g: Gen<_,_,_>) =
        resumeOrStartReader getReaderValue None g
    
    let toSeqState (g: Gen<_,_,_>) = toSeqStateReader ignore g
    
    let toSeqReader getReaderValue (g: Gen<_,_,_>) =
        toSeqStateReader getReaderValue g |> Seq.map fst

    let toSeq (g: Gen<_,_,_>) = toSeqReader ignore g

    let toListFx fx inputSeq =
        inputSeq |> toSeqFx fx |> Seq.toList
    
    let toList count gen =
        toSeq gen |> Seq.truncate count |> Seq.toList
