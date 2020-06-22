namespace FsLocalState

open FsLocalState.Core

module Eval =
    
    type Gen<'s, 'r, 'o> with
        static member DoIt() = ()
    
    let getValue (x: Res<_, _>) = x.value
    
    let getValues s = s |> Seq.map getValue

    module Eff =
    
        let toSeq2 getReaderValue (localWithInput: Eff<_, _, _, _>) =
            let mutable lastState: 'a option = None
            fun inputValues ->
                inputValues
                |> Seq.mapi (fun i v ->
                    let local = localWithInput v |> run
                    let res = local lastState (getReaderValue i)
                    lastState <- Some res.state
                    res)
    
        let toSeq getReaderValue (localWithInput: Eff<_, _, _, _>) =
            let evaluable = toSeq2 getReaderValue localWithInput
            fun inputValues -> evaluable inputValues |> getValues

    module Gen =
    
        let toSeq2 getReaderValue (local: Gen<_, _, _>) =
            
            // first. transform the gen to an effect
            let fx : Eff<_, _, _, _> =
                fun () -> local
    
            let evaluable = Eff.toSeq2 getReaderValue fx
            
            // now, we don't want to have a "seq<unit> -> seq<Res<_,_>>", but an "seq<Res<_,_>>"
            let inputSeq = Seq.initInfinite ignore
            let resultingValues = evaluable inputSeq
            resultingValues
    
        let toSeq getReaderValue (local: Gen<_, _, _>) =
            toSeq2 getReaderValue local |> getValues

    let toListn n s = s |> Seq.take n |> Seq.toList 
    
    let toArrayn n s = s |> Seq.take n |> Seq.toArray 
    