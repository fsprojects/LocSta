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
    | Gen of ('s option -> 'o list)

type Fx<'i, 'o, 's> =
    'i -> Gen<'o, 's>

[<Struct>]
type State<'rem, 'scurr, 'ssub> =
    { currState: 'scurr
      subState: 'ssub option
      remaining: 'rem list }


module Control =
    type Emit<'value> = Emit of 'value
    type EmitThenStop<'value> = EmitThenStop of 'value
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
        : Gen<GenResult<'o2, State<GenResult<'o1, 's1>, 's1, 's2>>, State<GenResult<'o1, 's1>, 's1, 's2>>
        =
        let evalf fgen mstate lastFState remaining =
            [ for res in (unwrap fgen) lastFState do
                match res with
                | GenResult.Emit (fres, fstate) ->
                    GenResult.Emit (fres, { currState = mstate; subState = Some fstate; remaining = remaining })
                | GenResult.DiscardWith fstate -> 
                    GenResult.DiscardWith { currState = mstate; subState = Some fstate; remaining = remaining }
                | GenResult.Discard ->
                    GenResult.DiscardWith { currState = mstate; subState = None; remaining = remaining }
                | GenResult.Stop -> 
                    GenResult.Stop
            ]
        let evalmres mres lastMState lastFState remaining =
            [
                match mres with
                | GenResult.Emit (mres, mstate) ->
                    let fgen = f mres
                    yield! evalf fgen mstate lastFState remaining
                | GenResult.DiscardWith stateM ->
                    yield GenResult.DiscardWith 
                        { currState = stateM; subState = lastFState; remaining = remaining }
                | GenResult.Discard ->
                    match lastMState with
                    | Some lastStateM ->
                        yield GenResult.DiscardWith 
                            { currState = lastStateM; subState = lastFState; remaining = remaining }
                    | None -> 
                        yield GenResult.Discard
                | GenResult.Stop ->
                    yield GenResult.Stop
            ]
        let rec evalm lastMState lastFState =
            match (unwrap m) lastMState with
            | res :: remaining ->
                evalmres res lastMState lastFState remaining
            | [] ->
                [ GenResult.Discard ]
        fun (state: State<_, 's1, 's2> option) ->
            let lastMState, lastFState, remaining =
                match state with
                | None -> None, None, []
                | Some v -> Some v.currState, v.subState, v.remaining
            match remaining with
            | x :: xs -> evalmres x lastMState lastFState xs
            | [] -> evalm lastMState lastFState
        |> create

    /// 'bindFdb' is invoked only ONCE per fdb { .. }.
    /// It takes a Gen<InitResult>, which is the first "let! state = init .." expression.
    /// The returned "feedback state" is then passed into f, which itself finally returns a
    /// Gen<FdbResult>.
    let bindFdb
        (f: 'f -> Gen<GenResult<'o, 'f>, 's>)
        (m: Init<'f>)
        : Gen<GenResult<'o, State<_, 'f, 's>>, State<_, 'f, 's>>
        =
        fun state ->
            let lastFeed, lastFState =
                match state with
                | None -> let (Init m) = m in m, None
                | Some v  -> v.currState, v.subState
            [ for res in (unwrap (f lastFeed)) lastFState do
                match res with
                | GenResult.Emit (fres, ffeed) ->
                    GenResult.Emit (fres, { currState = ffeed; subState = None; remaining = [] })
                | GenResult.DiscardWith ffeed ->
                    GenResult.DiscardWith { currState = ffeed; subState = None; remaining = [] }
                | GenResult.Discard ->
                    GenResult.DiscardWith { currState = lastFeed; subState = lastFState; remaining = [] }
                | GenResult.Stop ->
                    GenResult.Stop
            ]
        |> create


    // --------
    // return / yield
    // --------

    type SingletonState = private | SingletonState

    let ofSingletonValue term value =
        fun state ->
            match state with
            | None -> GenResult.Emit(value, SingletonState)
            | Some SingletonState -> term
        |> create

    let ofRepeatingValue (value: 'a) : Gen<_,_> =
        create (fun _ -> value)

    let returnValue value : Gen<_,_> =
        GenResult.Emit(value, ()) |> ofRepeatingValue
    let returnValueThenStop value : Gen<_,_> =
        ofSingletonValue GenResult.Stop value
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

    type Next = A | B | Done
    type CombineInfo<'sa, 'sb> =
        { astate: 'sa option
          bstate: 'sb option
          next: Next }

    let combine (a: Gen<GenResult<'o, 'sa>, 'sa>) (b: unit -> Gen<GenResult<'o, 'sb>, 'sb>) =
        let defaultState () = { astate = None; bstate = None; next = A }
        let getValue g state = (unwrap g) state
        fun state ->
            let state =  state |> Option.defaultWith defaultState
            let rec evala () =
                match getValue a state.astate with
                | GenResult.Emit (va, sa) -> GenResult.Emit (va, { state with astate = Some sa; next = A })
                | GenResult.Discard -> GenResult.DiscardWith { state with next = B }
                | GenResult.DiscardWith sa -> GenResult.DiscardWith { state with astate = Some sa; next = A }
                | GenResult.Stop -> evalb ()
            and evalb () =
                match getValue (b ()) state.bstate with
                | GenResult.Emit (vb, sb) -> GenResult.Emit (vb, { state with bstate = Some sb; next = B })
                | GenResult.Discard -> GenResult.DiscardWith { state with next = A }
                | GenResult.DiscardWith sb -> GenResult.DiscardWith { state with bstate = Some sb; next = B }
                | GenResult.Stop -> GenResult.Stop
            match state.next with
            | Done -> GenResult.Stop
            | A -> evala ()
            | B -> evalb ()
        |> create


    // --------
    // Builder
    // --------

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
        member _.Return(Control.Emit value) = returnValue value
        member _.Return(Control.EmitThenStop value) = returnValueThenStop value
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
            return Control.Emit result
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
            return Control.Emit (f l r)
        }
    
    let inline binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return Control.Emit (f l r)
        }
    
    let inline binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return Control.Emit (f l r)
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
