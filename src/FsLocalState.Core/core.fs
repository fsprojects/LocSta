namespace FsLocalState

type InitResult<'f> =
    | Init of 'f

[<RequireQualifiedAccess>]
type GenResult<'o, 's> =
    | ValueAndState of 'o * 's
    | Discard
    | DiscardWith of 's
    | Stop

type Gen<'o, 's> =
    | Gen of ('s option -> 'o)

type Fx<'i, 'o, 's> =
    'i -> Gen<'o, 's>

[<Struct>]
type State<'scurr, 'ssub> =
    { currState: 'scurr
      subState: 'ssub option }


module Res =
    type ValueAndLoop<'value> = ValueAndLoop of 'value
    type ValueAndStop<'value> = ValueAndStop of 'value
    type Feedback<'value, 'feedback> = Feedback of 'value * 'feedback
    type Discard = Discard
    type DiscardWith<'state> = DiscardWith of 'state
    type Stop = Stop


module Gen =
    
    let unwrap gen = let (Gen b) = gen in b


    // --------
    // Gen creation
    // --------

    /// Single case DU constructor.
    let create f = Gen f

    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let createWithSeed f seed =
        fun s ->
            let state = Option.defaultValue seed s
            f state
        |> create

    let createWithSeed2 seed f = createWithSeed seed f

    
    // --------
    // bind
    // --------

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
            | GenResult.ValueAndState (mres, mstate) ->
                let fGen = f mres
                match (unwrap fGen) lastFState with
                | GenResult.ValueAndState (fres, fstate) -> 
                    GenResult.ValueAndState (fres, { currState = mstate; subState = Some fstate })
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

    /// 'bindFdb' is invoked only ONCE per fdb { .. }.
    /// It takes a Gen<InitResult>, which is the first "let! state = init .." expression.
    /// The returned "feedback state" is then passed into f, which itself finally returns a
    /// Gen<FdbResult>.
    let bindFdb
        (f: 'f -> Gen<GenResult<'o, 'f>, 's>)
        (m: InitResult<'f>)
        : Gen<GenResult<'o, State<'f, 's>>, State<'f, 's>>
        =
        fun state ->
            let lastFeed, lastFState =
                match state with
                | None -> let (Init m) = m in m, None
                | Some { currState = feedback; subState = inner } -> feedback, inner
            let fgen = f lastFeed
            match (unwrap fgen) lastFState with
            | GenResult.ValueAndState (fres, ffeed) ->
                GenResult.ValueAndState (fres, { currState = ffeed; subState = None })
            | GenResult.DiscardWith ffeed ->
                GenResult.DiscardWith { currState = ffeed; subState = None }
            | GenResult.Discard ->
                GenResult.DiscardWith { currState = lastFeed; subState = lastFState }
            | GenResult.Stop ->
                GenResult.Stop
        |> create


    // --------
    // return / yield
    // --------

    type SingletonState = private | SingletonState

    let ofSingletonValue value =
        fun state ->
            match state with
            | None -> GenResult.ValueAndState(value, SingletonState)
            | Some SingletonState -> GenResult.Stop
        |> create

    let ofRepeatingValue (value: 'a) : Gen<_,_> =
        create (fun _ -> value)

    let ofValueAndLoop value : Gen<_,_> =
        GenResult.ValueAndState(value, ()) |> ofRepeatingValue
    let ofValueAndStop value : Gen<_,_> = 
        ofSingletonValue value
    let ofFeedback value feedback =
        GenResult.ValueAndState(value, feedback) |> ofRepeatingValue
    let ofDiscard<'a, 'b, 'c> : Gen<GenResult<'a, 'b>, 'c> =
        GenResult.Discard |> ofRepeatingValue
    let ofDiscardWith<'a, 's, 'c> (state: 's) : Gen<GenResult<'a, 's>, 'c> =
        GenResult.DiscardWith state |> ofRepeatingValue
    let ofStop<'a, 'b, 'c> : Gen<GenResult<'a, 'b>, 'c> =
        GenResult.Stop |> ofRepeatingValue


    // --------
    // singleton / seq / list
    // --------

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> createWithSeed2 (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> GenResult.ValueAndState (enumerator.Current, enumerator)
            | false -> GenResult.Stop
        )
        
    let ofList (l: list<_>) =
        l
        |> createWithSeed2 (fun l ->
            match l with
            | x::xs -> GenResult.ValueAndState (x, xs)
            | [] -> GenResult.Stop
        )


    // --------
    // combine
    // --------

    type CombineState<'sa, 'sb> =
        private
        | UseA of 'sa option 
        | UseB of 'sb option
    
    let combine (a: Gen<GenResult<'o, 'sa>, 'sa>) (b: unit -> Gen<GenResult<'o, 'sb>, 'sb>) =
        printfn "Combine"
        let b = b ()
        let getValue g state = (unwrap g) state
        fun state ->
            let state = state |> Option.defaultValue (UseA None)
            match state with
            | UseA lastSa ->
                match getValue a lastSa with
                | GenResult.ValueAndState (va, sa) -> GenResult.ValueAndState (va, UseA (Some sa))
                | GenResult.Discard -> GenResult.Discard
                | GenResult.DiscardWith sa -> GenResult.DiscardWith (UseA (Some sa))
                | GenResult.Stop -> GenResult.DiscardWith (UseB None)
            | UseB lastSb ->
                match getValue b lastSb with
                | GenResult.ValueAndState (vb, sb) -> GenResult.ValueAndState (vb, UseB (Some sb))
                | GenResult.Discard -> GenResult.Discard
                | GenResult.DiscardWith sb -> GenResult.DiscardWith (UseB (Some sb))
                | GenResult.Stop -> GenResult.Stop
        |> create

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.Zero() = ofDiscard
        member _.For(sequence: seq<'a>, body) = ofSeq sequence |> bind body
        member _.Combine(x, delayed) = combine x delayed
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type GenBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.YieldFrom(x) = ofList x
        // returns
        member _.Return(Res.ValueAndLoop value) = ofValueAndLoop value
        member _.Return(Res.ValueAndStop value) = ofValueAndStop value
        member _.Return(Res.Discard) = ofDiscard
        member _.Return(Res.DiscardWith state) = ofDiscardWith state
        member _.Return(Res.Stop) = ofStop
        
    type FeedbackBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindFdb f m
        // returns
        member _.Return(Res.Feedback (value, feedback)) = ofFeedback value feedback
        member _.Return(Res.Discard) = ofDiscard
        member _.Return(Res.DiscardWith state) = ofDiscardWith state
        member _.Return(Res.Stop) = ofStop
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()


    // --------
    // map / apply / transformation
    // --------

    let map projection x =
        fun state ->
            match (unwrap x) state with
            | GenResult.ValueAndState (x', state) -> GenResult.ValueAndState (projection x', state)
            | GenResult.DiscardWith s -> GenResult.DiscardWith s
            | GenResult.Discard -> GenResult.Discard
            | GenResult.Stop -> GenResult.Stop
        |> create

    let apply xGen fGen =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return Res.ValueAndLoop result
        }

    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen


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
            return Res.ValueAndLoop (f l r)
        }
    
    let inline binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return Res.ValueAndLoop (f l r)
        }
    
    let inline binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return Res.ValueAndLoop (f l r)
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
