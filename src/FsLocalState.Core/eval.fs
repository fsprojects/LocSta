[<AutoOpen>]
module FsLocalState.Eval

open FsLocalState

module Gen =

    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStart (state: 's option) (g: LoopGen<_,'s>) =
        let f = Gen.unwrap g
        let mutable state = state
        let mutable resume = true
        seq {
            while resume do
                for res in f state do
                    if resume then
                        match res with
                        | Res.Emit (LoopEmit (fres, fstate)) ->
                            state <- Some fstate
                            yield (fres, fstate)
                        | Res.DiscardWith (LoopDiscard fstate) ->
                            state <- Some fstate
                        | Res.Stop ->
                            resume <- false
        }
    
    let resume state (g: LoopGen<_,'s>) = resumeOrStart (Some state) g

    // TODO: Document this
    /// Be careful: This uses a state machine, which means:
    /// A mutable object is used as state!
    let toSeqStateFx (fx: Fx<'i,_,'s>) : seq<'i> -> seq<_ * 's> =
        let mutable state = None
        let mutable resume = true

        fun inputValues ->
            seq {
                let enumerator = inputValues.GetEnumerator()
                while enumerator.MoveNext() && resume do
                    let value = enumerator.Current
                    let fxres = Gen.unwrap (fx value) state
                    for res in fxres do
                        if resume then
                            match res with
                            | Res.Emit (LoopEmit (resF, stateF)) ->
                                state <- Some stateF
                                yield (resF, stateF)
                            | Res.DiscardWith (LoopDiscard stateF) ->
                                state <- Some stateF
                            | Res.Stop ->
                                resume <- false
            }

    let toSeqFx (fx: Fx<'i,_,'s>) : seq<'i> -> seq<_> =
        let evaluable = toSeqStateFx fx
        fun inputValues -> evaluable inputValues |> Seq.map fst
    
    let toSeqState (g: LoopGen<_,_>) = resumeOrStart None g
    
    let toSeq (g: LoopGen<_,_>) = toSeqState g |> Seq.map fst

    let toListFx fx inputSeq =
        inputSeq |> toSeqFx fx |> Seq.toList
    
    let toList gen =
        toSeq gen |> Seq.toList
    
    let toListn count gen =
        toSeq gen |> Seq.truncate count |> Seq.toList
