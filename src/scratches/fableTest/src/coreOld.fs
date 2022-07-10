
namespace LocSta

// TODO: struct tuples

type Gen<'o,'r,'s> = 'r -> 's option -> ('o * 's)

module Gen =
    
    let inline bind 
        ([<InlineIfLambda>] m: Gen<'o1,'r,'s1>)
        ([<InlineIfLambda>] f: 'o1 -> Gen<'o2,'r,'s2>)
        : Gen<'o2, 'r, 's1 * 's2>
        =
        fun r mfState ->
            // unpack the previous state (may be None or Some)
            let mState,fState =
                match mfState with
                | None -> None,None
                | Some (mState,fState) -> Some mState, Some fState

            // no modifications from here:
            // previousStateOfCurrentBlock and previousStateOfNextBlock are now
            // both optional, but block who use it can deal with that.

            // The result of currentBlock is made up of an actual value and a state that
            // has to be "recorded" by packing it together with the state of the
            // next block.
            let mOut,mState' = m r mState

            // Continue evaluating the computation:
            // passing the actual output value of currentBlock to the rest of the computation
            // gives us access to the next block in the computation:
            let fgen = f mOut

            // Evaluate the next block and build up the result of this bind function
            // as a block, so that it can be used as a bindable element itself -
            // but this time with state of 2 blocks packed together.
            let fOut,fState' = fgen r fState
            
            let resultingState = mState', fState'
            fOut, resultingState

    let inline ofValue x = fun r s -> x,()

    type GenBuilder() =
        member this.Bind(block, rest) = bind block rest
        member this.Return(x) = ofValue x
        member this.ReturnFrom(x) : Gen<_,_,_> = x

    let loop = GenBuilder()

    let inline map ([<InlineIfLambda>] f) x =
        loop {
            let! x' = x
            return f x'
        }

    let stateValue value : Gen<_,_,_> =
        fun r s ->
            let state = s |> Option.defaultValue value
            state,state

    let stateWith factory : Gen<_,_,_> =
        fun r s ->
            let state = s |> Option.defaultWith factory
            state,state

    let ofMutable initialValue : Gen<_,_,_> =
        fun r s ->
            let refCell = s |> Option.defaultWith (fun () -> ref initialValue)
            let setter = fun value -> refCell.contents <- value
            (refCell.contents, setter), refCell

    let inline toEvaluable ([<InlineIfLambda>] g: Gen<_,_,_>) =
        let mutable state = None
        fun r ->
            let fOut,fState = g r state
            state <- Some fState
            fOut

    // TODO: use toEvaluable
    let inline toSeq r ([<InlineIfLambda>] g) : seq<_> =
        let evaluable = toEvaluable g
        seq { while true do yield evaluable r  }

[<AutoOpen>]
module Autos =
    let loop = Gen.GenBuilder()
