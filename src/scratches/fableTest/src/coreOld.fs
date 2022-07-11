
namespace LocSta

// TODO: struct tuples

type Gen<'o,'s,'r> = 's option -> 'r -> ('o * 's)

module Gen =
    
    let inline bind 
        ([<InlineIfLambda>] m: Gen<'o1,'s1,'r>)
        ([<InlineIfLambda>] f: 'o1 -> Gen<'o2,'s2,'r>)
        : Gen<'o2, 's1 * 's2, 'r>
        =
        fun mfState r ->
            // unpack the previous state (may be None or Some)
            let mState,fState =
                match mfState with
                | None -> None,None
                | Some (mState,fState) -> Some mState, Some fState

            // The result of m is made up of an actual value and a state that
            // has to be "recorded" by packing it together with the state of the
            // next gen.
            let mOut,mState' = m mState r

            // Continue evaluating the computation:
            // passing the actual output value of m to the rest of the computation
            // gives us access to the next gen in the computation:
            let fgen = f mOut

            // Evaluate the next gen and build up the result of this bind function
            // as a gen, so that it can be used as a bindable element itself -
            // but this time with state of 2 gens packed together.
            let fOut,fState' = fgen fState r

            let resultingState = mState', fState'
            fOut, resultingState
    
    (*
    type [<Struct>] BoxedState =
        { stateType: Type; state: obj }
        static member Create(state) = { stateType = state.GetType(); state = state }
    type [<Struct>] CombinedBoxedState =
        { mState: BoxedState; fState: BoxedState }
    
    let private unboxState<'t> state =
        match state with
        | None -> None
        | Some x -> Some (x.state :?> 't)

    let inline bindBoxed
        ([<InlineIfLambda>] m: Gen<'o1,'s1,'r>)
        ([<InlineIfLambda>] f: 'o1 -> Gen<'o2,'s2,'r>)
        : Gen<'o2, CombinedBoxedState, 'r>
        =
        fun mfState r ->
            let mState,fState =
                match mfState with
                | None -> None,None
                | Some mfState -> Some mfState.mState, Some mfState.fState
            let mOut,mState' = m (unboxState mState) r
            let fgen = f mOut
            let fOut,fState' = fgen (unboxState fState) r
            let resultingState =
                { mState = BoxedState.Create(mState')
                  fState = BoxedState.Create(fState') }
            fOut, resultingState
    *)

    let inline ofValue x = fun s r -> x,()

    type GenBuilder() =
        member _.Return(x) = ofValue x
        member inline _.Bind(m, [<InlineIfLambda>] f) = bind m f
        member _.ReturnFrom(x) : Gen<_,_,_> = x

    let loop = GenBuilder()

    let inline map proj ([<InlineIfLambda>] g) = fun s r ->
        let o,s = g s r in proj o, s

    let preserve factory = fun s r ->
        let state = s |> Option.defaultWith factory
        state,state

    let ofMutable initialValue = fun s r ->
        let refCell = s |> Option.defaultWith (fun () -> ref initialValue)
        let setter = fun value -> refCell.contents <- value
        (refCell.contents, setter), refCell

    let inline toEvaluable ([<InlineIfLambda>] g: Gen<_,_,_>) =
        let mutable state = None
        fun r ->
            let fOut,fState = g state r
            state <- Some fState
            fOut

    // TODO: use toEvaluable
    let inline toSeq r ([<InlineIfLambda>] g) =
        let evaluable = toEvaluable g
        seq { while true do yield evaluable r  }

    let inline withState ([<InlineIfLambda>] g) = fun s r ->
        let o,s = g s r in (o,s),s

[<AutoOpen>]
module Autos =
    let loop = Gen.GenBuilder()
