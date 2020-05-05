
type Combined<'a, 'b> = { current: 'a; next: 'b }

type BlockOutput<'value, 'state> =
    { value: 'value
      state: 'state }

type Block<'outValue, 'state> =
    | Block of ('state option -> BlockOutput<'outValue, 'state>)

let runB block = let (Block x) = block in x

// we cannot make this a DU because comp exp won't work with that.
type BlockWithInput<'inp, 'outp, 'state> = ('inp -> Block<'outp, 'state>)


module Block =

    let bind
            (currentBlock: Block<'outA, 'stateA>)
            (rest        : BlockWithInput<'outA, 'outB, 'stateB>)
                         : Block<'outB, Combined<'stateA, 'stateB>> =

        Block <| fun combinedState ->

            let unpack maybeCombined =
                match maybeCombined with
                | None          -> None,None
                | Some combined -> Some combined.current, Some combined.next

            // unpack the previous state (may be None or Some)
            let stateOfCurrentBlock,stateOfNextBlock = unpack combinedState

            // no modifications from here:
            // previousStateOfCurrentBlock and previousStateOfNextBlock are now
            // both optional, but block who use it can deal with that.

            // The result of currentBlock is made up of an actual value and a state that
            // has to be "recorded" by packing it together with the state of the
            // next block.
            let currentBlockOutput = (runB currentBlock) stateOfCurrentBlock

            // Continue evaluating the computation:
            // passing the actual output value of currentBlock to the rest of the computation
            // gives us access to the next block in the computation:
            let nextBlock = rest currentBlockOutput.value

            // Evaluate the next block and build up the result of this bind function
            // as a block, so that it can be used as a bindable element itself -
            // but this time with state of 2 blocks packed together.
            let nextBlockOutput = (runB nextBlock) stateOfNextBlock
            
            let result =
                { value = nextBlockOutput.value
                  state =
                    { current = currentBlockOutput.state
                      next = nextBlockOutput.state } }
            
            result

    let inline ret x =
        Block <| fun _ -> { value = x; state = () }    

    // TODO: standard builder methods
    type BlockBuilder() =
        member this.Bind(block, rest) = bind block rest
        member this.Return(x) = ret x
        member this.ReturnFrom(x) : Block<_,_> = x
        
    let private block = BlockBuilder()

    let (>>=) = bind
    
    let kleisli
            (f: BlockWithInput<'inA,'outA,_>)
            (g: BlockWithInput<'outA,'outB,_>)
              : BlockWithInput<'inA,'outB,_> =
        fun x -> block {
            let! f' = f x
            return! g f'
        }
        
    let (>=>) = kleisli

    let kleisliPipe x f =
        block {
            let! f' = f x
            return f'
        }
        
    let (|=>) = kleisliPipe

    let binaryOp
            (a:  BlockWithInput<'inp, 'outA, _>)
            (b:  BlockWithInput<'inp, 'outB, _>)
            (op: 'outA -> 'outB -> 'outOp)
            : BlockWithInput<'inp, 'outOp, _> =
        fun dpIn -> block {
            let! a' = a dpIn
            let! b' = b dpIn
            return op a' b'
        }

    let ( &&& ) a b = binaryOp a b (&&)
    let ( ||| ) a b = binaryOp a b (||)

    let combineTwoInputs
            (a:  BlockWithInput<'inp, 'outA, _>)
            (b:  BlockWithInput<'inp, 'outB, _>)
            (op: 'outA -> 'outB -> 'outOp)
            : 'inp -> 'inp -> Block<'outOp, _> =
        let rule inpA inpB = block {
            let! a' = a inpA
            let! b' = b inpB
            return (op a' b')
        }
        rule

    let ( <&> ) a b = combineTwoInputs a b (&&)
    let ( <|> ) a b = combineTwoInputs a b (||)


[<AutoOpen>]
module Feedback =

    type Fbd<'fbdValue, 'value> = { feedback: 'fbdValue; out: 'value }

    let feedback (f: BlockWithInput<'fbdValue, Fbd<'fbdValue,'value>,'state>) seed =
        Block <| fun state ->
            let myState,innerState = 
                match state with
                | None            -> seed,None
                | Some (my,inner) -> my,inner
            let block = f myState
            let blockRes = (runB block) innerState
            let feed = blockRes.value
            let innerState = blockRes.state

            { value = feed.out
              state = feed.feedback, Some innerState }

    let (<->) seed f = feedback f seed
