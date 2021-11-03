namespace FsLocalState

type Init<'f> =
    | Init of 'f

[<RequireQualifiedAccess>]
type Control<'v, 's> =
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


module Res =
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
        (f: 'o1 -> Gen<Control<'o2, 's2>, 's2>) 
        (m: Gen<Control<'o1, 's1>, 's1>)
        : Gen<Control<'o2, State<'s1, 's2>>, State<'s1, 's2>>
        =
        fun (state: State<'s1, 's2> option) ->
            let lastMState, lastFState =
                match state with
                | None -> None, None
                | Some v -> Some v.currState, v.subState
            match (unwrap m) lastMState with
            | Control.Emit (mres, mstate) ->
                let fGen = f mres
                match (unwrap fGen) lastFState with
                | Control.Emit (fres, fstate) ->
                    Control.Emit (fres, { currState = mstate; subState = Some fstate })
                | Control.DiscardWith stateF -> 
                    Control.DiscardWith { currState = mstate; subState = Some stateF }
                | Control.Discard ->
                    Control.DiscardWith { currState = mstate; subState = None }
                | Control.Stop -> 
                    Control.Stop
            | Control.DiscardWith stateM ->
                Control.DiscardWith { currState = stateM; subState = lastFState }
            | Control.Discard ->
                match lastMState with
                | Some lastStateM -> Control.DiscardWith { currState = lastStateM; subState = lastFState }
                | None -> Control.Discard
            | Control.Stop ->
                Control.Stop
        |> create

    /// 'bindFdb' is invoked only ONCE per fdb { .. }.
    /// It takes a Gen<InitResult>, which is the first "let! state = init .." expression.
    /// The returned "feedback state" is then passed into f, which itself finally returns a
    /// Gen<FdbResult>.
    let bindFdb
        (f: 'f -> Gen<Control<'o, 'f>, 's>)
        (m: Init<'f>)
        : Gen<Control<'o, State<'f, 's>>, State<'f, 's>>
        =
        fun state ->
            let lastFeed, lastFState =
                match state with
                | None -> let (Init m) = m in m, None
                | Some { currState = feedback; subState = inner } -> feedback, inner
            let fgen = f lastFeed
            match (unwrap fgen) lastFState with
            | Control.Emit (fres, ffeed) ->
                Control.Emit (fres, { currState = ffeed; subState = None })
            | Control.DiscardWith ffeed ->
                Control.DiscardWith { currState = ffeed; subState = None }
            | Control.Discard ->
                Control.DiscardWith { currState = lastFeed; subState = lastFState }
            | Control.Stop ->
                Control.Stop
        |> create


    // --------
    // return / yield
    // --------

    type SingletonState = private | SingletonState

    let ofSingletonValue value =
        fun state ->
            match state with
            | None -> Control.Emit(value, SingletonState)
            | Some SingletonState -> Control.Stop
        |> create

    let ofRepeatingValue (value: 'a) : Gen<_,_> =
        create (fun _ -> value)

    let returnValueAndLoop value : Gen<_,_> =
        Control.Emit(value, ()) |> ofRepeatingValue
    let returnValueAndStop value : Gen<_,_> = 
        ofSingletonValue value
    let returnFeedback value feedback =
        Control.Emit(value, feedback) |> ofRepeatingValue
    let returnDiscard<'a, 'b, 'c> : Gen<Control<'a, 'b>, 'c> =
        Control.Discard |> ofRepeatingValue
    let returnDiscardWith<'a, 's, 'c> (state: 's) : Gen<Control<'a, 's>, 'c> =
        Control.DiscardWith state |> ofRepeatingValue
    let returnStop<'a, 'b, 'c> : Gen<Control<'a, 'b>, 'c> =
        Control.Stop |> ofRepeatingValue


    // --------
    // singleton / seq / list
    // --------

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> createWithSeed2 (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> Control.Emit (enumerator.Current, enumerator)
            | false -> Control.Stop
        )
        
    let ofList (l: list<_>) =
        l
        |> createWithSeed2 (fun l ->
            match l with
            | x::xs -> Control.Emit (x, xs)
            | [] -> Control.Stop
        )


    // --------
    // combine
    // --------

    type CombineState<'sa, 'sb> =
        private
        | UseA of 'sa option 
        | UseB of 'sb option
    
    let combine (a: Gen<Control<'o, 'sa>, 'sa>) (b: unit -> Gen<Control<'o, 'sb>, 'sb>) =
        printfn "Combine"
        let b = b ()
        let getValue g state = (unwrap g) state
        fun state ->
            let state = state |> Option.defaultValue (UseA None)
            match state with
            | UseA lastSa ->
                match getValue a lastSa with
                | Control.Emit (va, sa) -> Control.Emit (va, UseA (Some sa))
                | Control.Discard -> Control.Discard
                | Control.DiscardWith sa -> Control.DiscardWith (UseA (Some sa))
                | Control.Stop -> Control.DiscardWith (UseB None)
            | UseB lastSb ->
                match getValue b lastSb with
                | Control.Emit (vb, sb) -> Control.Emit (vb, UseB (Some sb))
                | Control.Discard -> Control.Discard
                | Control.DiscardWith sb -> Control.DiscardWith (UseB (Some sb))
                | Control.Stop -> Control.Stop
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
        member _.Return(Res.EmitAndLoop value) = returnValueAndLoop value
        member _.Return(Res.EmitAndStop value) = returnValueAndStop value
        member _.Return(Res.Discard) = returnDiscard
        member _.Return(Res.DiscardWith state) = returnDiscardWith state
        member _.Return(Res.Stop) = returnStop
        
    type FeedbackBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindFdb f m
        // returns
        member _.Return(Res.Feedback (value, feedback)) = returnFeedback value feedback
        member _.Return(Res.Discard) = returnDiscard
        member _.Return(Res.DiscardWith state) = returnDiscardWith state
        member _.Return(Res.Stop) = returnStop
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()


    // --------
    // map / apply / transformation
    // --------

    let mapValueAndState (proj: 'v -> 's -> 'o) (inputGen: Gen<_,_>) =
        fun state ->
            match (unwrap inputGen) state with
            | Control.Emit (v,s) -> Control.Emit (proj v s, s)
            | Control.DiscardWith s -> Control.DiscardWith s
            | Control.Discard -> Control.Discard
            | Control.Stop -> Control.Stop
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
            return Res.EmitAndLoop result
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
            return Res.EmitAndLoop (f l r)
        }
    
    let inline binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return Res.EmitAndLoop (f l r)
        }
    
    let inline binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return Res.EmitAndLoop (f l r)
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
