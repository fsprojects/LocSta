
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

    let inline ofValue x = fun s r -> x,()

    type GenBuilder() =
        member this.Bind(m, f) = bind m f
        member this.Return(x) = ofValue x
        member this.ReturnFrom(x) : Gen<_,_,_> = x

    let loop = GenBuilder()

    let inline map ([<InlineIfLambda>] f) x =
        loop {
            let! x' = x
            return f x'
        }

    let preserve factory : Gen<_,_,_> =
        fun s r ->
            let state = s |> Option.defaultWith factory
            state,state

    let ofMutable initialValue : Gen<_,_,_> =
        fun s r ->
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
    let inline toSeq r ([<InlineIfLambda>] g) : seq<_> =
        let evaluable = toEvaluable g
        seq { while true do yield evaluable r  }

[<AutoOpen>]
module Autos =
    let loop = Gen.GenBuilder()
