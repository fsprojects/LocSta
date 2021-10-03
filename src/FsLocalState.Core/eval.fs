[<AutoOpen>]
module FsLocalState.Gen.Eval

open FsLocalState
    
let getValue = fst
let getValues s = s |> Seq.map getValue


// TODO: same pettern (resumeOrStart, etc.) as in Gen also for Fx

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

let resume state (g: Gen<_,_,_>) = resumeReader ignore g

let toStateSeqReaderFx (getReaderValue: int -> 'r) (eff: Eff<_,_,_,'r>) =
    let mutable lastState = None
    fun inputValues ->
        seq {
            for i,v in inputValues |> Seq.indexed do
                let local = eff v |> Gen.run
                match local lastState (getReaderValue i) with
                | Some res ->
                    lastState <- Some (snd res)
                    yield res
                | None -> ()
        }

let toStateSeqFx (eff: Eff<_,_,_,_>) = toStateSeqReaderFx ignore eff

let toSeqReaderFx  (getReaderValue: int -> 'r) (eff: Eff<_,_,_,'r>) =
    let evaluable = toStateSeqReaderFx getReaderValue eff
    fun inputValues -> evaluable inputValues |> getValues
    
let toSeqFx (eff: Eff<_,_,_,_>) = toSeqReaderFx ignore eff
    
let toStateSeqReader getReaderValue (g: Gen<_,_,_>) =
    resumeOrStartReader getReaderValue None g
    
let toStateSeq (g: Gen<_,_,_>) = toStateSeqReader ignore g
    
let toSeqReader getReaderValue (g: Gen<_,_,_>) =
    toStateSeqReader getReaderValue g |> Seq.map fst

let toSeq (g: Gen<_,_,_>) = toSeqReader ignore g

let toListFx eff inputSeq =
    inputSeq |> toSeqFx eff |> Seq.toList
    
let toList count gen =
    toSeq gen |> Seq.truncate count |> Seq.toList
