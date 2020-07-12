namespace FsLocalState

open FsLocalState

[<AutoOpen>]
module Eval =
    
    let getValue (x: Res<_, _>) = x.value
    let getValues s = s |> Seq.map getValue

    module Eff =
        // TODO: same pettern (resumeOrStart, etc.) as in Gen
        
        let toSeqWithState getReaderValue (localWithInput: Eff<_, _, _, _>) =
            let mutable lastState: 'a option = None
            fun inputValues ->
                inputValues
                |> Seq.mapi (fun i v ->
                    let local = localWithInput v |> run
                    let res = local lastState (getReaderValue i)
                    lastState <- Some res.state
                    res)

        let toSeq getReaderValue (localWithInput: Eff<_, _, _, _>) =
            let evaluable = toSeqWithState getReaderValue localWithInput
            fun inputValues -> evaluable inputValues |> getValues

    module Gen =
        let resumeOrStart getReaderValue state (x: Gen<'v, 's, 'r>) =
            let f = Gen.run x
            let mutable state = state
            seq {
                while true do
                    let res = f state (getReaderValue())
                    state <- Some res.state
                    res
            }
        
        let resume getReaderValue state (x: Gen<'v, 's, 'r>) =
            resumeOrStart getReaderValue (Some state) x

        let toSeqWithState getReaderValue (x: Gen<'v, 's, 'r>) =
            resumeOrStart getReaderValue None x
            
        let toSeq getReaderValue (x: Gen<'v, 's, 'r>) =
            toSeqWithState getReaderValue x |> Seq.map (fun x -> x.value)

module Seq =
    let toListN n s = s |> Seq.take n |> Seq.toList 
    let toArrayN n s = s |> Seq.take n |> Seq.toArray 
