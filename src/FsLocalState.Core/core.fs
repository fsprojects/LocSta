namespace FsLocalState

type Init<'f> =
    | Init of 'f

[<RequireQualifiedAccess>]
type GenResult<'v, 's> =
    | Emit of 'v * 's
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


module Control =
    type EmitAndLoop<'value> = EmitAndLoop of 'value
    type EmitAndStop<'value> = EmitAndStop of 'value
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
            | GenResult.Emit (mres, mstate) ->
                let fGen = f mres
                match (unwrap fGen) lastFState with
                | GenResult.Emit (fres, fstate) ->
                    GenResult.Emit (fres, { currState = mstate; subState = Some fstate })
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
        (m: Init<'f>)
        : Gen<GenResult<'o, State<'f, 's>>, State<'f, 's>>
        =
        fun state ->
            let lastFeed, lastFState =
                match state with
                | None -> let (Init m) = m in m, None
                | Some { currState = feedback; subState = inner } -> feedback, inner
            let fgen = f lastFeed
            match (unwrap fgen) lastFState with
            | GenResult.Emit (fres, ffeed) ->
                GenResult.Emit (fres, { currState = ffeed; subState = None })
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
            | None -> GenResult.Emit(value, SingletonState)
            | Some SingletonState -> GenResult.Stop
        |> create

    let ofRepeatingValue (value: 'a) : Gen<_,_> =
        create (fun _ -> value)

    let returnValueAndLoop value : Gen<_,_> =
        GenResult.Emit(value, ()) |> ofRepeatingValue
    let returnValueAndStop value : Gen<_,_> = 
        ofSingletonValue value
    let returnFeedback value feedback =
        GenResult.Emit(value, feedback) |> ofRepeatingValue
    let returnDiscard<'a, 'b, 'c> : Gen<GenResult<'a, 'b>, 'c> =
        GenResult.Discard |> ofRepeatingValue
    let returnDiscardWith<'a, 's, 'c> (state: 's) : Gen<GenResult<'a, 's>, 'c> =
        GenResult.DiscardWith state |> ofRepeatingValue
    let returnStop<'a, 'b, 'c> : Gen<GenResult<'a, 'b>, 'c> =
        GenResult.Stop |> ofRepeatingValue


    // --------
    // singleton / seq / list
    // --------

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> createWithSeed2 (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> GenResult.Emit (enumerator.Current, enumerator)
            | false -> GenResult.Stop
        )
        
    let ofList (l: list<_>) =
        l
        |> createWithSeed2 (fun l ->
            match l with
            | x::xs -> GenResult.Emit (x, xs)
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
                | GenResult.Emit (va, sa) -> GenResult.Emit (va, UseA (Some sa))
                | GenResult.Discard -> GenResult.Discard
                | GenResult.DiscardWith sa -> GenResult.DiscardWith (UseA (Some sa))
                | GenResult.Stop -> GenResult.DiscardWith (UseB None)
            | UseB lastSb ->
                match getValue b lastSb with
                | GenResult.Emit (vb, sb) -> GenResult.Emit (vb, UseB (Some sb))
                | GenResult.Discard -> GenResult.Discard
                | GenResult.DiscardWith sb -> GenResult.DiscardWith (UseB (Some sb))
                | GenResult.Stop -> GenResult.Stop
        |> create

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.Zero() = returnDiscard
        member _.For(sequence: seq<'a>, body) = ofSeq sequence |> bind body
        member _.Combine(x, delayed) = combine x delayed
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type GenBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.YieldFrom(x) = ofList x
        // returns
        member _.Return(Control.EmitAndLoop value) = returnValueAndLoop value
        member _.Return(Control.EmitAndStop value) = returnValueAndStop value
        member _.Return(Control.Discard) = returnDiscard
        member _.Return(Control.DiscardWith state) = returnDiscardWith state
        member _.Return(Control.Stop) = returnStop
        
    type FeedbackBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindFdb f m
        // returns
        member _.Return(Control.Feedback (value, feedback)) = returnFeedback value feedback
        member _.Return(Control.Discard) = returnDiscard
        member _.Return(Control.DiscardWith state) = returnDiscardWith state
        member _.Return(Control.Stop) = returnStop
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()


    // --------
    // map / apply / transformation
    // --------

    let mapValueAndState (proj: 'v -> 's -> 'o) (inputGen: Gen<_,_>) =
        fun state ->
            match (unwrap inputGen) state with
            | GenResult.Emit (v,s) -> GenResult.Emit (proj v s, s)
            | GenResult.DiscardWith s -> GenResult.DiscardWith s
            | GenResult.Discard -> GenResult.Discard
            | GenResult.Stop -> GenResult.Stop
        |> create

    let mapValue proj (inputGen: Gen<_,_>) =
        mapValueAndState (fun v _ -> proj v) inputGen

    let includeState (inputGen: Gen<_,_>) =
        mapValueAndState (fun v s -> v,s) inputGen

    let apply xGen fGen =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return Control.EmitAndLoop result
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
            return Control.EmitAndLoop (f l r)
        }
    
    let inline binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return Control.EmitAndLoop (f l r)
        }
    
    let inline binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return Control.EmitAndLoop (f l r)
        }

type Gen<'o,'s> with
    static member inline (+) (left, right) = Gen.binOpBoth left right (+)
    static member inline (-) (left, right) = Gen.binOpBoth left right (-)
    static member inline (*) (left, right) = Gen.binOpBoth left right (*)
    static member inline (/) (left, right) = Gen.binOpBoth left right (/)
    static member inline (%) (left, right) = Gen.binOpBoth left right (%)
    static member inline (==) (left, right) = Gen.binOpBoth left right (=)
    
    static member inline (+) (left: float, right) = Gen.binOpLeft left right (+)
    static member inline (-) (left: float, right) = Gen.binOpLeft left right (-)
    static member inline (*) (left: float, right) = Gen.binOpLeft left right (*)
    static member inline (/) (left: float, right) = Gen.binOpLeft left right (/)
    static member inline (%) (left: float, right) = Gen.binOpLeft left right (%)
    static member inline (==) (left: float, right) = Gen.binOpLeft left right (=)

    static member inline (+) (left: int, right) = Gen.binOpLeft left right (+)
    static member inline (-) (left: int, right) = Gen.binOpLeft left right (-)
    static member inline (*) (left: int, right) = Gen.binOpLeft left right (*)
    static member inline (/) (left: int, right) = Gen.binOpLeft left right (/)
    static member inline (%) (left: int, right) = Gen.binOpLeft left right (%)
    static member inline (==) (left: int, right) = Gen.binOpLeft left right (=)

    static member inline (+) (left, right: float) = Gen.binOpRight left right (+)
    static member inline (-) (left, right: float) = Gen.binOpRight left right (-)
    static member inline (*) (left, right: float) = Gen.binOpRight left right (*)
    static member inline (/) (left, right: float) = Gen.binOpRight left right (/)
    static member inline (%) (left, right: float) = Gen.binOpRight left right (%)
    static member inline (==) (left, right: float) = Gen.binOpRight left right (=)

    static member inline (+) (left, right: int) = Gen.binOpRight left right (+)
    static member inline (-) (left, right: int) = Gen.binOpRight left right (-)
    static member inline (*) (left, right: int) = Gen.binOpRight left right (*)
    static member inline (/) (left, right: int) = Gen.binOpRight left right (/)
    static member inline (%) (left, right: int) = Gen.binOpRight left right (%)
    static member inline (==) (left, right: int) = Gen.binOpRight left right (=)

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
