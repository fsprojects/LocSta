
module Local =
        
    type Res<'a, 'b> =
        { value: 'a
          state: 'b }

    type Local<'value, 'state, 'reader> =
        Local of ('state option -> 'reader -> Res<'value, 'state>)

    // TODO: seems to be impossible having a single case DU here?
    type LocalInput<'inp, 'value, 'state, 'reader> =
        'inp -> Local<'value, 'state, 'reader>

    type StateAcc<'a, 'b> =
        { mine: 'a
          exess: 'b }
    
    let run m = let (Local b) = m in b

    let bind (m: Local<'a, 'sa, 'r>) (f: 'a -> Local<'b, 'sb, 'r>): Local<'b, StateAcc<'sa, 'sb>, 'r> =
        let localFunc localState readerState =
            let unpackedLocalState =
                match localState with
                | None ->
                    { mine = None
                      exess = None }
                | Some v ->
                    { mine = Some v.mine
                      exess = Some v.exess }

            let m' = (run m) unpackedLocalState.mine readerState
            let fLocal = f m'.value
            let f' = (run fLocal) unpackedLocalState.exess readerState

            { value = f'.value
              state =
                { mine = m'.state
                  exess = f'.state } }

        Local localFunc

    let ret x =
        Local(fun _ _ ->
            { value = x
              state = () })

    // TODO: Docu
    // TODO: other builder methods
    type LocalReaderBuilder<'a>() =
        member __.Bind(m: Local<_, _, 'a>, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom x = x

    // TODO: other builder methods
    type LocalBuilder() =
        member __.Bind(m, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom x = x

    let local = LocalBuilder()

    let kleisli (f: LocalInput<'a, 'b, _, _>) (g: LocalInput<'b, 'c, _, _>) : LocalInput<'a, 'c, _, _> =
        fun x -> local {
            let! f' = f x
            return! g f'
        }
    let (>=>) = kleisli

    let kleisliPipe (f: Local<'a, _, _>) (g: LocalInput<'a, 'b, _, _>) : Local<'b, _, _> =
        local {
            let! f' = f
            return! g f'
        }
    let (|=>) = kleisliPipe

    let mapB local mapping =
        let f' s r =
            let res = (run local) s r
            let mappedRes = mapping res.value
            { value = mappedRes
              state = res.state }
        Local f'

    /// map operator
    let (<!>) = mapB

    let apply (f: Local<'v1 -> 'v2, _, 'r>) (l: Local<'v1, _, 'r>): Local<'v2, _, 'r> =
        local {
            let! l' = l
            let! f' = f
            let result = f' l'
            return result
        }

    /// apply operator        
    let (<*>) = apply


    let inline binOpLeftRight left right f =
        local {
            let! l = left
            let! r = right
            return f l r }

    type Local<'v, 's, 'r> with
        static member inline (+) (left, right) = binOpLeftRight left right (+)
        static member inline (-) (left, right) = binOpLeftRight left right (-)
        static member inline (*) (left, right) = binOpLeftRight left right (*)
        static member inline (/) (left, right) = binOpLeftRight left right (/)
        static member inline (%) (left, right) = binOpLeftRight left right (%)

    let inline binOpLeft left right f =
        local {
            let l = left
            let! r = right
            return f l r
        }

    type Local<'v, 's, 'r> with
        static member inline (+) (left, right) = binOpLeft left right (+)
        static member inline (-) (left, right) = binOpLeft left right (-)
        static member inline (*) (left, right) = binOpLeft left right (*)
        static member inline (/) (left, right) = binOpLeft left right (/)
        static member inline (%) (left, right) = binOpLeft left right (%)

    let inline binOpRight left right f =
        local {
            let! l = left
            let r = right
            return f l r
        }

    type Local<'v, 's, 'r> with
        static member inline (+) (left, right) = binOpRight left right (+)
        static member inline (-) (left, right) = binOpRight left right (-)
        static member inline (*) (left, right) = binOpRight left right (*)
        static member inline (/) (left, right) = binOpRight left right (/)


    /// Reads the global state that is passed around to every loop function.
    let read() =
        Local(fun _ r ->
            { value = r
              state = () })

    /// Lifts a function with an initial value.
    let liftSeed seed block =
        fun s r ->
            let x =
                match s with
                | Some previousState -> previousState
                | None -> seed
            block x r


[<AutoOpen>]
module Feedback =

    [<Struct>]
    type Fbd<'a, 'b> =
        { feedback: 'a
          out: 'b }

    /// Feedback with reader state
    let (++>) seed (f: 'a -> 'r -> Local<Fbd<'a, 'v>, 's, 'r>) =
        let f1 =
            fun prev r ->
                let myPrev, innerPrev =
                    match prev with
                    | None -> seed, None
                    | Some(my, inner) -> my, inner

                let lRes = runBlock (f myPrev r) innerPrev r
                let feed = lRes.value
                let innerState = lRes.state
                { value = feed.out
                  state = feed.feedback, Some innerState }
        Local f1

    /// Feedback without reader state
    let (+->) seed f = (++>) seed (fun s _ -> f s)


[<AutoOpen>]
module Helper =

    let listN x n =
        x
        |> Seq.take n
        |> Seq.toList


[<AutoOpen>]
module Eval =

    let getValues (s: Res<_, _> seq) = s |> Seq.map (fun x -> x.value)

    let noReader = fun _ -> ()

    module Effect =

        /// Converts a block into a sequence with the given state.
        /// The getReaderState function is called for each evaluation.
        let toSeqSV getReaderState (blockWithInput: 'inp -> Local<_, _, _>) =
            let mutable lastState: 'a option = None
            fun inputValues ->
                inputValues
                |> Seq.mapi (fun i v ->
                    let block = blockWithInput v |> runBlock
                    let res = block lastState (getReaderState i)
                    lastState <- Some res.state
                    res)

        /// Converts a block into a sequence with the given state.
        /// The getReaderState function is called for each evaluation.
        let toSeqV getReaderState (blockWithInput: 'inp -> Local<_, _, _>) =
            fun inputValues -> toSeqSV getReaderState blockWithInput inputValues |> getValues

    module Generator =

        /// Converts a block into a sequence with the given state.
        /// The getReaderState function is called for each evaluation.
        let toSeqSV getReaderState (blockWithInput: Local<_, _, _>) =
            Effect.toSeqSV getReaderState (fun () -> blockWithInput) (Seq.initInfinite (fun _ -> ())) |> getValues

        /// Converts a block into a sequence with the given state.
        /// The getReaderState function is called for each evaluation.
        let toSeqV getReaderState (blockWithInput: Local<_, _, _>) =
            toSeqSV getReaderState blockWithInput |> getValues

    module Test =
        let evalN block =
            Generator.toSeqSV noReader block |> listN




[<AutoOpen>]
module Audio =

    type Env =
        { samplePos: int
          sampleRate: int }

    let toSeconds env = (double env.samplePos) / (double env.sampleRate)

    let block = LocalReaderBuilder<Env>()

    module Eval =

        /// Converts a block and a given sample rate to a sequence.
        let toAudioSeq (b: Local<_, _, Env>) sampleRate =
            b
            |> Eval.Generator.toSeqSV (fun i ->
                { samplePos = i
                  sampleRate = sampleRate })

        /// Converts a block with a sample rate of 44.1kHz to a sequence.
        let toAudioSeq44k (b: Local<_, _, _>) = toAudioSeq b 44100

        module Test =

            let evalN sr block =
                toAudioSeq block sr |> listN

            let evalN44k block =
                toAudioSeq44k block |> listN

