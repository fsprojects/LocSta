namespace FsLocalState

open FsLocalState

[<AutoOpen>]
module Eval =
    
    module Eff =
        // TODO: same pettern (resumeOrStart, etc.) as in Gen
        
        let toSeqWithState getReaderValue (localWithInput: Eff<_, _, _, _>) =
            let mutable lastState: 'a option = None
            fun inputValues ->
                inputValues
                |> Seq.mapi (fun i v ->
                    let local = localWithInput v |> run
                    let res = local lastState (getReaderValue i)
                    match res with
                    | Some res ->
                        lastState <- Some (snd res)
                        Some res
                    | None -> None)
                |> Seq.choose id

        let toSeq getReaderValue (localWithInput: Eff<_, _, _, _>) =
            let evaluable = toSeqWithState getReaderValue localWithInput
            fun inputValues -> evaluable inputValues |> Seq.map fst

    module Gen =
        let resumeOrStart getReaderValue state (x: Gen<'v, 's, 'r>) =
            let f = Gen.run x
            let mutable state = state
            seq {
                while true do
                    let res = f state (getReaderValue())
                    match res with
                    | Some res ->
                        state <- Some (snd res)
                        yield res
                    | None -> ()
            }
        
        let resume getReaderValue state (x: Gen<'v, 's, 'r>) =
            resumeOrStart getReaderValue (Some state) x

        let toSeqWithState getReaderValue (x: Gen<'v, 's, 'r>) =
            resumeOrStart getReaderValue None x
            
        let toSeq getReaderValue (x: Gen<'v, 's, 'r>) =
            toSeqWithState getReaderValue x |> Seq.map fst

module Seq =
    let toListN n s = s |> Seq.take n |> Seq.toList 
    let toArrayN n s = s |> Seq.take n |> Seq.toArray 
