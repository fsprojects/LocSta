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
        
        let toSeq2 getReaderValue (x: Gen<'v, 's, 'r>) =
            seq {
                let mutable i = 0
                let mutable lastState: 's option = None
                while true do
                    let (Gen f) = x
                    let res = f lastState (getReaderValue i)
                    
                    lastState <- Some res.state
                    i <- i + 1
                    
                    res
            }
            
        let toSeq getReaderValue (x: Gen<'v, 's, 'r>) =
            toSeq2 getReaderValue x |> Seq.map (fun x -> x.value)

    let toListn n s = s |> Seq.take n |> Seq.toList 
    
    let toArrayn n s = s |> Seq.take n |> Seq.toArray 
    