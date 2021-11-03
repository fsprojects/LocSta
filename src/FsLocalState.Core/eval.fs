[<AutoOpen>]
module FsLocalState.Eval

open FsLocalState

module Gen =

    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStart (state: 's option) (g: Gen<_,'s>) =
        let f = Gen.unwrap g
        let mutable state = state
        let mutable resume = true
        seq {
            while resume do
                match f state with
                | GenResult.Emit (resF, stateF) ->
                    state <- Some stateF
                    yield (resF, stateF)
                | GenResult.DiscardWith stateF ->
                    state <- Some stateF
                | GenResult.Discard -> 
                    ()
                | GenResult.Stop ->
                    resume <- false
                | GenResult.Reset ->
                    state <- None
        }
    
    let resume state (g: Gen<_,'s>) = resumeOrStart (Some state) g

    let toSeqStateFx (fx: Fx<'i,_,'s>) : seq<'i> -> seq<_ * 's> =
        let mutable state = None
        let mutable resume = true

        fun inputValues ->
            seq {
                let enumerator = inputValues.GetEnumerator()
                while enumerator.MoveNext() && resume do
                    let value = enumerator.Current
                    let local = fx value |> Gen.unwrap
                    match local state with
                    | GenResult.Emit (resF, stateF) ->
                        state <- Some stateF
                        yield (resF, stateF)
                    | GenResult.DiscardWith stateF ->
                        state <- Some stateF
                    | GenResult.Discard ->
                        ()
                    | GenResult.Stop ->
                        resume <- false
                    | GenResult.Reset ->
                        state <- None
            }

    let toSeqFx (fx: Fx<'i,_,'s>) : seq<'i> -> seq<_> =
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

////open System.Collections
////open System.Collections.Generic

////type Gen<'o, 's> with
////    interface IEnumerable with
////        member this.GetEnumerator() : IEnumerator = failwith ""
////    interface IEnumerable<'o> with
////        member this.GetEnumerator() : IEnumerator<'o> = failwith ""
