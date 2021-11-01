namespace FsLocalState

type InitResult<'f> =
    | Init of 'f

type GenResult<'o, 's> =
    | ValueAndState of 'o * 's
    | Discard
    | DiscardWith of 's
    | Stop

module Res =
    let value value = ValueAndState(value, ())
    let feedback value feedback = ValueAndState(value, feedback)
    let discard : GenResult<'o, 's> = Discard
    let discardWith state : GenResult<'o, 's> = DiscardWith state
    let stop : GenResult<'o, 's> = Stop

type Gen<'o, 's> =
    | Gen of ('s option -> 'o)

type Fx<'i, 'o, 's> =
    'i -> Gen<'o, 's>

[<Struct>]
type GenState<'sCurr, 'sSub> =
    { currState: 'sCurr
      subState: 'sSub option }

module Gen =
    let unwrap gen = let (Gen b) = gen in b

    /// Single case DU constructor.
    let create f = Gen f

    let bind
        (f: 'o1 -> Gen<GenResult<'o2, 's2>, 's2>) 
        (m: Gen<GenResult<'o1, 's1>, 's1>)
        : Gen<GenResult<'o2, GenState<'s1, 's2>>, GenState<'s1, 's2>>
        =
        fun (state: GenState<'s1, 's2> option) ->
            let lastMState, lastFState =
                match state with
                | None -> None, None
                | Some v -> Some v.currState, v.subState
            match (unwrap m) lastMState with
            | ValueAndState (mres, mstate) ->
                let fGen = f mres
                match (unwrap fGen) lastFState with
                | ValueAndState (fres, fstate) -> 
                    ValueAndState (fres, { currState = mstate; subState = Some fstate })
                | DiscardWith stateF -> 
                    DiscardWith { currState = mstate; subState = Some stateF }
                | Discard ->
                    DiscardWith { currState = mstate; subState = None }
                | Stop -> 
                    Stop
            | DiscardWith stateM ->
                DiscardWith { currState = stateM; subState = lastFState }
            | Discard ->
                match lastMState with
                | Some lastStateM -> DiscardWith { currState = lastStateM; subState = lastFState }
                | None -> Discard
            | Stop ->
                Stop
        |> create

    /// 'bindFdb' is invoked only ONCE per fdb { .. }.
    /// It takes a Gen<InitResult>, which is the first "let! state = init .." expression.
    /// The returned "feedback state" is then passed into f, which itself finally returns a
    /// Gen<FdbResult>.
    let bindFdb
        (f: 'f -> Gen<GenResult<'o, 'f>, 's>)
        (m: InitResult<'f>)
        //: Gen<GenResult<'o2, FdbState<'f, GenState<unit, 's2>>>, FdbState<'f, GenState<unit, 's2>>>
        =
        fun state ->
            let lastFeed, lastFState =
                match state with
                | None -> let (Init m) = m in m, None
                | Some { currState = feedback; subState = inner } -> feedback, inner
            let fgen = f lastFeed
            match (unwrap fgen) lastFState with
            | ValueAndState (fres, ffeed) ->
                ValueAndState (fres, { currState = ffeed; subState = None })
            | DiscardWith ffeed ->
                DiscardWith { currState = ffeed; subState = None }
            | Discard ->
                DiscardWith { currState = lastFeed; subState = lastFState }
            | Stop ->
                Stop
        |> create
        
    // TODO: this is an infinite value. There's no way of returning a single value in gen mode.
    /// Wraps a BaseResult into a gen.
    let ofValue x : Gen<_,_> = create (fun _ -> x)

    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let createWithSeed f seed =
        fun s ->
            let state = Option.defaultValue seed s
            f state
        |> create

    let createWithSeed2 seed f = createWithSeed seed f

    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> createWithSeed2 (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> ValueAndState (enumerator.Current, enumerator)
            | false -> Stop
        )
        
    let ofList (l: list<_>) =
        l
        |> createWithSeed2 (fun l ->
            match l with
            | x::xs -> ValueAndState (x, xs)
            | [] -> Stop
        )

    type Combined<'sa, 'sb> = 
        | UseA of 'sa option 
        | UseB of 'sb option

    let combine a b =
        printfn "Combine"
        let getValue g state = (unwrap g) state
        fun state ->
            let state = state |> Option.defaultValue (UseA None)
            match state with
            | UseA state ->
                match getValue a state with
                | ValueAndState (va, sa) ->
                    ValueAndState (va, UseA (Some sa))
                | Discard
                | DiscardWith _ -> Discard
                | Stop -> DiscardWith (UseB None)
            | UseB state ->
                match getValue (b ()) state with
                | ValueAndState (vb, sb) ->
                    ValueAndState (vb, UseB (Some sb))
                | Discard
                | DiscardWith _ -> Discard
                | Stop -> Stop
        |> create

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = x
        member _.Zero() = ofValue Discard
        member _.For(sequence: seq<'a>, body) = ofSeq sequence |> bind body
        member _.Combine(x, delayed) = combine x delayed
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type GenBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.Return(x: GenResult<_,_>) = ofValue x
        member this.Yield(x: GenResult<_,_>) = this.Return(x)
        
    type FeedbackBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindFdb f m
        member _.Return(x: GenResult<_,_>) = ofValue x
        member this.Yield(x: GenResult<_,_>) = this.Return(x)
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()


    // --------
    // map / apply
    // --------

    let map projection x =
        fun state ->
            match (unwrap x) state with
            | ValueAndState (x', state) -> ValueAndState (projection x', state)
            | DiscardWith s -> DiscardWith s
            | Discard -> Discard
            | Stop -> Stop
        |> create

    let apply xGen fGen =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return Res.value result
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
            return Res.value (f l r)
        }
    
    let inline binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return Res.value (f l r)
        }
    
    let inline binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return Res.value (f l r)
        }

type Gen<'o,'s> with
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
