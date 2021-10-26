[<AutoOpen>]
module FsLocalState.Eval

open FsLocalState

module Gen =

    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStart (state: 's option) (g: Gen<'o, 's>) =
        let f = Gen.run g
        let mutable state = state
        let mutable resume = true
        seq {
            while resume do
                match f state with
                | Value (resF, stateF) ->
                    state <- Some stateF
                    yield (resF, stateF)
                | Discard (Some stateF) ->
                    state <- Some stateF
                | Discard None ->
                    ()
                | Stop ->
                    resume <- false
        }
    
    let resume state (g: Gen<'o, 's>) = resumeOrStart (Some state) g

    let toSeqStateFx (fx: Fx<'i, 'o, 's>) : seq<'i> -> seq<'o * 's> =
        let mutable state = None
        let mutable resume = true

        fun inputValues ->
            seq {
                let enumerator = inputValues.GetEnumerator()
                while enumerator.MoveNext() && resume do
                    let value = enumerator.Current
                    let local = fx value |> Gen.run
                    match local state with
                    | Value (resF, stateF) ->
                        state <- Some stateF
                        yield (resF, stateF)
                    | Discard (Some stateF) ->
                        state <- Some stateF
                    | Discard None ->
                        ()
                    | Stop ->
                        resume <- false
            }

    let toSeqFx (fx: Fx<'i, 'o, 's>) : seq<'i> -> seq<'o> =
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
