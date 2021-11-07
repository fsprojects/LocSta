[<AutoOpen>]
module FsLocalState.Eval

open FsLocalState

module Gen =

    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStart (state: 's option) (g: GenForGen<_,'s>) =
        let f = Gen.unwrap g
        let mutable state = state
        let mutable resume = true
        seq {
            while resume do
                for res in f state do
                    if resume then
                        match res with
                        | GenResult.Emit (GenEmit (fres, fstate)) ->
                            state <- Some fstate
                            yield (fres, fstate)
                        | GenResult.DiscardWith (GenDiscard fstate) ->
                            state <- Some fstate
                        | GenResult.Stop ->
                            resume <- false
        }
    
    let resume state (g: GenForGen<_,'s>) = resumeOrStart (Some state) g

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
                            | GenResult.Emit (GenEmit (resF, stateF)) ->
                                state <- Some stateF
                                yield (resF, stateF)
                            | GenResult.DiscardWith (GenDiscard stateF) ->
                                state <- Some stateF
                            | GenResult.Stop ->
                                resume <- false
            }

    let toSeqFx (fx: Fx<'i,_,'s>) : seq<'i> -> seq<_> =
        let evaluable = toSeqStateFx fx
        fun inputValues -> evaluable inputValues |> Seq.map fst
    
    let toSeqState (g: GenForGen<_,_>) = resumeOrStart None g
    
    let toSeq (g: GenForGen<_,_>) = toSeqState g |> Seq.map fst

    let toListFx fx inputSeq =
        inputSeq |> toSeqFx fx |> Seq.toList
    
    let toList gen =
        toSeq gen |> Seq.toList
    
    let toListn count gen =
        toSeq gen |> Seq.truncate count |> Seq.toList
