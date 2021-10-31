[<AutoOpen>]
module FsLocalState.Core

[<RequireQualifiedAccess>]
type InitResult<'o, 'f> =
    | Init of 'o * 'f

[<RequireQualifiedAccess>]
type GenResult<'o, 's> =
    | Value of 'o * 's
    | Discard
    | DiscardWith of 's
    | Stop

[<RequireQualifiedAccess>]
type FdbResult<'o, 'f> =
    | Feedback of 'o * 'f
    | Discard
    | DiscardWith of 'f
    | Stop

type Gen<'o, 's> =
    | Gen of ('s option -> 'o)

type Fx<'i, 'o, 's> =
    'i -> Gen<'o, 's>

[<Struct>]
type State<'sCurr, 'sSub> = 
    { currState: 'sCurr
      subState: 'sSub option }

[<Struct>] 
type FeedbackState<'f, 's> = 
    { feedback: 'f
      inner: 's option }

module Gen =
    let unwrap gen = let (Gen b) = gen in b

    /// Single case DU constructor.
    let create f = Gen f

    let bind
        (f: 'o1 -> Gen<GenResult<'o2, 's2>, 's2>) 
        (m: Gen<GenResult<'o1, 's1>, 's1>)
        : Gen<GenResult<'o2, State<'s1, 's2>>, State<'s1, 's2>>
        =
        fun (state: State<'s1, 's2> option) ->
            let lastMState, lastFState =
                match state with
                | None -> None, None
                | Some v -> Some v.currState, v.subState
            match (unwrap m) lastMState with
            | GenResult.Value (mres, mstate) ->
                let fGen = f mres
                match (unwrap fGen) lastFState with
                | GenResult.Value (fres, fstate) -> 
                    GenResult.Value (fres, { currState = mstate; subState = Some fstate })
                | GenResult.DiscardWith stateF -> 
                    GenResult.DiscardWith { currState = mstate; subState = Some stateF }
                | GenResult.Discard ->
                    GenResult.DiscardWith { currState = mstate; subState = None }
                | GenResult.Stop -> 
                    GenResult.Stop
            | GenResult.DiscardWith stateM ->
                GenResult.DiscardWith { currState = stateM; subState = lastFState }
            | GenResult.Discard ->
                match lastMState with
                | Some lastStateM -> GenResult.DiscardWith { currState = lastStateM; subState = lastFState }
                | None -> GenResult.Discard
            | GenResult.Stop ->
                GenResult.Stop
        |> create

    /// 'bindFdb' is invoked only ONCE per fdb { .. } with
    /// the first "let! state = init .." exp returning an InitResult.
    let bindFdb
        (f: 'o1 -> Gen<FdbResult<'o2, 'f option>, 's2>)
        (m: 'f option -> Gen<InitResult<'o1, 'f>, unit>)
        : _ // TODO
        =
        fun state ->
            let lastFeed, lastMState, lastFState =
                match state with
                | None -> None, None, None
                | Some { feedback = feedback; inner = inner } ->
                    match inner with
                    | None -> feedback, None, None
                    | Some v -> feedback, Some v.currState, v.subState
            let mgen = m lastFeed
            match (unwrap mgen) lastMState with
            | InitResult.Init (mres, mfeed) ->
                // TODO: mf is discarded - that sound ok
                let fgen = f mres
                match (unwrap fgen) lastFState with
                | FdbResult.Feedback (fres, ffeed) ->
                    GenResult.Value (
                        fres, 
                        { feedback = ffeed
                          inner = Some { currState = ()
                                         subState = None } }
                    )
                | FdbResult.DiscardWith ffeed ->
                    GenResult.DiscardWith
                        { feedback = ffeed
                          inner = Some { currState = ()
                                         subState = None } }
                | FdbResult.Discard ->
                    GenResult.DiscardWith
                        { feedback = lastFeed
                          inner = Some { currState = ()
                                         subState = lastFState } }
                | FdbResult.Stop -> GenResult.Stop
        |> create
        
    /// Wraps a BaseResult into a gen.
    let ofValue x : Gen<_,_> = create (fun _ -> x)

    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let ofSeed f seed =
        fun s ->
            let state = Option.defaultValue seed s
            f state
        |> create

    let ofSeed2 seed f = ofSeed seed f

    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> ofSeed2 (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> GenResult.Value (enumerator.Current, enumerator)
            | false -> GenResult.Stop
        )
        
    let ofList (l: list<_>) =
        l
        |> ofSeed2 (fun l ->
            match l with
            | x::xs -> GenResult.Value (x, xs)
            | [] -> GenResult.Stop
        )

    // TODO: other builder methods
    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = x
        member _.Zero() = ofValue GenResult.Discard
        member _.For (sequence: seq<'a>, body) = ofSeq sequence |> bind body

    // TODO: other builder methods
    type GenBuilder() =
        inherit BaseBuilder()
        
        // builder methods
        member _.Bind(m, f) = bind f m 
        member _.Return(x) = ofValue x
        member this.Yield(x) = this.Return(x)
        
        // result ctors
        member _.value(v) = GenResult.Value (v, ())
        member _.discard<'a, 'b>() = GenResult.Discard
        member _.discard(state) = GenResult.DiscardWith state
        member _.stop<'a, 'b>() = GenResult.Stop

    type FeedbackBuilder() =
        inherit BaseBuilder()
        
        // builder methods
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindFdb f m
        member _.Return(x) = ofValue x
        member this.Yield(x) = this.Return(x)
        
        // result ctors
        member _.value value feedback = FdbResult.Feedback (value, Some feedback)
        member _.discard() : FdbResult<'a, 'f> = FdbResult.Discard
        member _.discardWith(feedback: 'f) : FdbResult<'a, 'f> = FdbResult.DiscardWith feedback
        member _.stop() : FdbResult<'a, 'f> = FdbResult.Stop
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()
    

    // --------
    // feedback
    // --------

    /// Initialized the `fdb { .. }` workflow.
    let inline init seed =
        fun feedback -> gen {
            let feedback = feedback |> Option.defaultValue seed
            return InitResult.Init (feedback, feedback)
        }


    // --------
    // map / apply
    // --------

    let map projection x =
        fun state ->
            match (unwrap x) state with
            | GenResult.Value (x', state) -> GenResult.Value (projection x', state)
            | GenResult.DiscardWith s -> GenResult.DiscardWith s
            | GenResult.Discard -> GenResult.Discard
            | GenResult.Stop -> GenResult.Stop
        |> create

    let apply xGen fGen =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return gen.value result
        }


    // -------
    // Kleisli
    // -------

    let pipe (g: Fx<_,_,_>) (f: Gen<_,_>) : Gen<_,_> =
        gen {
            let! f' = f
            return! g f' 
        }

    let pipeFx (g: Fx<_,_,_>) (f: Fx<_,_,_>): Fx<_,_,_> =
        fun x -> gen {
            let! f' = f x
            return! g f' 
        }


    // ----------
    // Arithmetik
    // ----------

    let inline binOpBoth left right f =
        gen {
            let! l = left
            let! r = right
            return gen.value (f l r)
        }
    
    let inline binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return gen.value (f l r)
        }
    
    let inline binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return gen.value (f l r)
        }

type Gen<'v, 's> with
    static member inline (+) (left, right) = Gen.binOpBoth left right (+)
    static member inline (-) (left, right) = Gen.binOpBoth left right (-)
    static member inline (*) (left, right) = Gen.binOpBoth left right (*)
    static member inline (/) (left, right) = Gen.binOpBoth left right (/)
    static member inline (%) (left, right) = Gen.binOpBoth left right (%)
    
    static member inline (+) (left: float, right) = Gen.binOpLeft left right (+)
    static member inline (-) (left: float, right) = Gen.binOpLeft left right (-)
    static member inline (*) (left: float, right) = Gen.binOpLeft left right (*)
    static member inline (/) (left: float, right) = Gen.binOpLeft left right (/)
    static member inline (%) (left: float, right) = Gen.binOpLeft left right (%)

    static member inline (+) (left: int, right) = Gen.binOpLeft left right (+)
    static member inline (-) (left: int, right) = Gen.binOpLeft left right (-)
    static member inline (*) (left: int, right) = Gen.binOpLeft left right (*)
    static member inline (/) (left: int, right) = Gen.binOpLeft left right (/)
    static member inline (%) (left: int, right) = Gen.binOpLeft left right (%)

    static member inline (+) (left, right: float) = Gen.binOpRight left right (+)
    static member inline (-) (left, right: float) = Gen.binOpRight left right (-)
    static member inline (*) (left, right: float) = Gen.binOpRight left right (*)
    static member inline (/) (left, right: float) = Gen.binOpRight left right (/)
    static member inline (%) (left, right: float) = Gen.binOpRight left right (%)

    static member inline (+) (left, right: int) = Gen.binOpRight left right (+)
    static member inline (-) (left, right: int) = Gen.binOpRight left right (-)
    static member inline (*) (left, right: int) = Gen.binOpRight left right (*)
    static member inline (/) (left, right: int) = Gen.binOpRight left right (/)
    static member inline (%) (left, right: int) = Gen.binOpRight left right (%)

[<AutoOpen>]
module TopLevelOperators =

    /// Kleisli operator (fx >> fx)
    let (>=>) f g = Gen.pipeFx g f

    /// Kleisli "pipe" operator (gen >> fx)
    let (|=>) f g = Gen.pipe g f

    /// Bind operator
    let (>>=) m f = Gen.bind f m

let gen = Gen.gen
let fdb = Gen.fdb
let init = Gen.init
