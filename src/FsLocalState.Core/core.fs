[<AutoOpen>]
module FsLocalState.Core

type GenFunc<'output, 'state> =
    'state option -> ('output * 'state) option

type Gen<'output, 'state> =
    | Gen of GenFunc<'output, 'state>

// TODO: seems to be impossible having a single case DU here?
type Fx<'input, 'output, 'state> =
    'input -> Gen<'output, 'state>

[<Struct>]
type State<'a, 'b> = { currState: 'a; subState: 'b }

module Gen =

    let internal run gen = let (Gen b) = gen in b

    // Creates a Gen from a function that takes optional state.
    let create f = Gen f
    
    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let ofSeed f seed =
        fun s ->
            let state = Option.defaultValue seed s
            f state
        |> create

    let ofSeed2 seed f = ofSeed seed f

    let bind (f: 'o1 -> Gen<'o2, 's2>) (m: Gen<'o1, 's1>) : Gen<'o2, State<'s1, 's2>> =
        fun (s: State<'s1, 's2> option) ->
            let unpackedLocalState =
                match s with
                | None -> { currState = None; subState = None }
                | Some v -> { currState = Some v.currState; subState = Some v.subState }
            match (run m) unpackedLocalState.currState with
            | Some m' ->
                let fGen = fst m' |> f
                match (run fGen) unpackedLocalState.subState with
                | Some f' -> Some (fst f', { currState = snd m'; subState = snd f' })
                | None -> None
            | None -> None
        |> create

    type FeedbackState<'mine, 'inner> = { mine: 'mine; inner: 'inner option }

    let feedback
        (seed: 'workingState)
        (f: 'workingState -> Gen<'output * 'workingState, 'innerState>)
        : Gen<'output, FeedbackState<'workingState, 'innerState>>
        =
        fun (s: FeedbackState<'workingState, 'innerState> option) ->
            let feedbackState, innerState =
                match s with
                | None -> seed, None
                | Some { mine = mine; inner = inner } -> mine, inner
            match run (f feedbackState) innerState with
            | Some res ->
                let feed = fst res
                let innerState = snd res
                Some (fst feed, { mine = snd feed; inner = Some innerState })
            | None -> None
        |> create
        
    let zero () =
        fun _ -> None
        |> create

    let ofValue x =
        fun _ -> Some (x, ())
        |> create
    
    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> ofSeed2 (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> Some (enumerator.Current, enumerator)
            | false -> None
        )

    let ofList (l: list<_>) =
        l
        |> ofSeed2 (fun l ->
            match l with
            | x::xs -> Some (x, xs)
            | [] -> None
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
    
    
    // --------
    // map / apply
    // --------

    //let map projection x =
    //    gen {
    //        let! res = x
    //        return projection res
    //    }

    let map (projection: 'a -> 'b) (x: Gen<'a, 's>) : Gen<'b, 's> =
        fun state ->
            match (run x) state with
            | Some (x', state) -> Some (projection x', state)
            | None -> None
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

    let kleisli (g: Fx<'a, 'b, _>) (f: Gen<'a, _>) : Gen<'b, _> =
        gen {
            let! f' = f
            return! g f' 
        }

    let kleisliFx (g: Fx<'b, 'c, _>) (f: Fx<'a, 'b, _>): Fx<'a, 'c, _> =
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

    /// Feedback with reader state
    let (=>) seed f = Gen.feedback seed f

    /// Kleisli operator (fx >> fx)
    let (>=>) f g = Gen.kleisliFx g f

    /// Kleisli "pipe" operator (gen >> fx)
    let (|=>) f g = Gen.kleisli g f

let gen = Gen.gen
