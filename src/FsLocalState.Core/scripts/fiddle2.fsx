
    
type Res<'a, 'b> =
    { value: 'a
      state: 'b }

module Res =
    let getValue res = res.value

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

let local<'a> = LocalReaderBuilder<'a>()

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
    fun _ (r: 'r) -> { value = r; state = () }
    |> Local


// TODO: Example
/// Lifts a function with an initial value.
let liftSeed seed local =
    fun s r ->
        let x =
            match s with
            | Some previousState -> previousState
            | None -> seed
        local x r
    |> Local


[<AutoOpen>]
module Feedback =

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

                let lRes = run (f myPrev r) innerPrev r
                let feed = lRes.value
                let innerState = lRes.state
                { value = feed.out
                  state = feed.feedback, Some innerState }
        Local f1



[<AutoOpen>]
module Eval =

    let noReader = fun _ -> ()
    
    let private getValues s = s |> Seq.map Res.getValue

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqi getReaderValue (localWithInput: LocalInput<_,_,_,_>) =
        let mutable lastState: 'a option = None
        fun inputValues ->
            inputValues
            |> Seq.mapi (fun i v ->
                let local = localWithInput v |> run
                let res = local lastState (getReaderValue i)
                lastState <- Some res.state
                res)

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeq getReaderValue (localWithInput: LocalInput<_,_,_,_>) =
        toSeqi (fun _ -> getReaderValue()) localWithInput

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqValuesi getReaderValue (localWithInput: LocalInput<_,_,_,_>) =
        fun inputValues ->
            toSeqi getReaderValue localWithInput inputValues
            |> getValues

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqValues getReaderValue (localWithInput: LocalInput<_,_,_,_>) =
        fun inputValues ->
            toSeq getReaderValue localWithInput inputValues
            |> getValues

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqGen2 getReaderValue (local: Local<_,_,_>) =
        Seq.initInfinite (fun _ -> ())
        |> toSeq getReaderValue (fun () -> local)

    /// Converts a local into a sequence with the given state.
    /// The getReaderState function is called for each evaluation.
    let toSeqGen getReaderValue (local: Local<_,_,_>) =
        toSeqGen2 getReaderValue local
        |> getValues

    let toListn n seq =
        seq
        |> Seq.take n
        |> Seq.toList



//[<AutoOpen>]
//module Audio =
//
//    type Env =
//        { samplePos: int
//          sampleRate: int }
//
//    let toSeconds env = (double env.samplePos) / (double env.sampleRate)
//
//    module Eval =
//
//        /// Converts a signal and a given sample rate to a sequence.
//        let toAudioSeq (local: Local<_, _, Env>) sampleRate =
//            local
//            |> Eval.Generator.toSeqSV (fun i ->
//                { samplePos = i
//                  sampleRate = sampleRate })
//
//        /// Converts a signal with a sample rate of 44.1kHz to a sequence.
//        let toAudioSeq44k (local: Local<_, _, _>) = toAudioSeq local 44100
//
//        module Test =
//
//            let evalN sr local =
//                toAudioSeq local sr |> listN
//
//            let evalN44k local =
//                toAudioSeq44k local |> listN
//
