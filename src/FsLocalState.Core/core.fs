[<AutoOpen>]
module FsLocalState.Core

type GenResult<'output, 'state> =
    | Value of 'output * 'state
    | Discard of 'state
    | Stop

type GenFunc<'output, 'state> =
    'state option -> GenResult<'output, 'state>

type Gen<'output, 'state> =
    | Gen of GenFunc<'output, 'state>

// TODO: seems to be impossible having a single case DU here?
type Fx<'input, 'output, 'state> =
    'input -> Gen<'output, 'state>

[<Struct>]
type State<'a, 'b> = { currState: 'a; subState: 'b option }

module Gen =

    let internal run gen = let (Gen b) = gen in b

    // Creates a Gen from a function that takes optional state.
    let create f = Gen f
    
    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let ofSeed f seed =
        fun s ->
            printfn $"state is: {s}"
            let state = Option.defaultValue seed s
            f state
        |> create

    let bind (f: 'o1 -> Gen<'o2, 's2>) (m: Gen<'o1, 's1>) : Gen<'o2, State<'s1, 's2>> =
        fun (s: State<'s1, 's2> option) ->
            let unpackedLocalState =
                match s with
                | None -> { currState = None; subState = None }
                | Some v -> { currState = Some v.currState; subState = v.subState }
            match (run m) unpackedLocalState.currState with
            | Value (m', sm') ->
                let fGen = f m'
                match (run fGen) unpackedLocalState.subState with
                | Value (f', sf') -> Value (f', { currState = sm'; subState = Some sf' })
                | Discard sf' -> Discard { currState = sm'; subState = Some sf' }
                | Stop -> Stop
            | Discard sm' -> Discard { currState = sm'; subState = unpackedLocalState.subState }
            | Stop -> Stop
        |> create

    type FeedbackState<'mine, 'inner> = { mine: 'mine; inner: 'inner option }

    //let feedback
    //    (seed: 'workingState)
    //    (f: 'workingState -> Gen<'output * 'workingState, 'innerState>)
    //    : Gen<'output, FeedbackState<'workingState, 'innerState>>
    //    =
    //    fun (s: FeedbackState<'workingState, 'innerState> option) ->
    //        let feedbackState, innerState =
    //            match s with
    //            | None -> seed, None
    //            | Some { mine = mine; inner = inner } -> mine, inner
    //        match run (f feedbackState) innerState with
    //        | Value res ->
    //            let feed = fst res
    //            let innerState = snd res
    //            Value (fst feed, { mine = snd feed; inner = Some innerState })
    //        | Discard s -> Discard s
    //        | Stop -> Stop
    //    |> create
        
    let zero () =
        fun _ -> Discard ()
        |> create

    let ofValue x =
        fun _ -> Value (x, ())
        |> create
    
    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> ofSeed (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> Value (enumerator.Current, enumerator)
            | false -> Stop
        )

    let ofList (l: list<_>) =
        l |> ofSeed (fun l ->
            match l with
            | x::xs -> Value (x, xs)
            | [] -> Stop
        )

    // TODO: other builder methods
    type GenBuilder() =
        member this.Bind(m: Gen<_, _>, f: _ -> Gen<_, _>) = bind f m
        member this.Return(x) = ofValue x
        member this.ReturnFrom(x) = x
        member this.Zero() = zero ()
        member this.Yield(x) = this.Return(x)
        member this.YieldFrom(x) = x
        //member this.Delay(f) = f
        //member this.Run(f) = f ()
        //member this.While (guard, body) =
        //    if not (guard()) 
        //    then this.Zero() 
        //    else
        //        this.Bind(body(), fun () ->
        //            this.While (guard, body))  
        member this.TryWith (body, handler) =
            try this.ReturnFrom(body())
            with e -> handler e
        member this.TryFinally (body, compensation) =
            try this.ReturnFrom(body ())
            finally compensation () 
        member this.Using (disposable: #System.IDisposable, body) =
            let body' = fun () -> body disposable
            this.TryFinally(body', fun () -> 
                match disposable with 
                    | null -> () 
                    | disp -> disp.Dispose())
        //member this.For (sequence: seq<_>, body) =
        //    this.Using(sequence.GetEnumerator(), fun enum -> 
        //        this.While(enum.MoveNext, 
        //            this.Delay(fun () -> body enum.Current)))

    let gen = GenBuilder()
    
    // this will introduce more state, so we use the version below
    //let map projection x =
    //    gen {
    //        let! res = x
    //        return projection res
    //    }
    let map (projection: 'a -> 'b) (x: Gen<'a, 's>) : Gen<'b, 's> =
        fun state ->
            match (run x) state with
            | Value (x', state) -> Value (projection x', state)
            | Discard state -> Discard state
            | Stop -> Stop
        |> create

    let apply (xGen: Gen<'o1, _>) (fGen: Gen<'o1 -> 'o2, _>) : Gen<'o2, _> =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return result
        }


    // -------
    // Kleisli
    // -------

    let pipe (g: Fx<'a, 'b, _>) (f: Gen<'a, _>) : Gen<'b, _> =
        gen {
            let! f' = f
            return! g f' 
        }

    let pipeFx (g: Fx<'b, 'c, _>) (f: Fx<'a, 'b, _>): Fx<'a, 'c, _> =
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
            return f l r }
    
    let inline binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return f l r
        }
    
    let inline binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return f l r
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
module Operators =

    ///// Feedback with reader state
    //let (=>) seed f = Gen.feedback seed f

    /// Kleisli operator (fx >> fx)
    let (>=>) f g = Gen.pipeFx g f

    /// Kleisli "pipe" operator (gen >> fx)
    let (|=>) f g = Gen.pipe g f

let gen = Gen.gen
