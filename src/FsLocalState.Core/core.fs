[<AutoOpen>]
module FsLocalState.Core

type GenResult<'output, 'state> =
    | Value of 'output * 'state
    | Discard of 'state option
    | Stop

type GenFunc<'output, 'state> =
    'state option -> GenResult<'output, 'state>

type Gen<'output, 'state> =
    | Gen of GenFunc<'output, 'state>

type Fx<'input, 'output, 'state> =
    'input -> Gen<'output, 'state>

[<Struct>]
type State<'a, 'b> = { currState: 'a; subState: 'b option }

[<Struct>]
type FeedbackState<'mine, 'inner> = { mine: 'mine; inner: 'inner option }

type Res<'a> = Res of 'a

module Res =
    let value v = Value (v, ()) |> Res
    let discard<'a, 'b> : Res<GenResult<'a, 'b>> = Discard None |> Res
    let discardWith state = Discard (Some state) |> Res
    let stop<'a, 'b> : Res<GenResult<'a, 'b>> = Stop |> Res
    let feedback value feedback = Value ((value, feedback), ()) |> Res

module Gen =
    let asFunc gen = let (Gen b) = gen in b

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
        fun (state: State<'s1, 's2> option) ->
            let lastMState, lastFState =
                match state with
                | None -> None, None
                | Some v -> Some v.currState, v.subState
            match (asFunc m) lastMState with
            | Value (mres, mstate) ->
                let fGen = f mres
                match (asFunc fGen) lastFState with
                | Value (fres, fstate) -> Value (fres, { currState = mstate; subState = Some fstate })
                | Discard stateF -> Discard (Some { currState = mstate; subState = stateF })
                | Stop -> Stop
            | Discard (Some stateM) -> Discard (Some { currState = stateM; subState = lastFState })
            | Discard None ->
                match lastMState with
                | Some lastStateM -> Discard (Some { currState = lastStateM; subState = lastFState })
                | None -> Discard None
            | Stop -> Stop
        |> create

    let bindFdb (f: 'o1 -> Gen<('o2 * 'f option), 's2>) (m: 'f option -> Gen<('o1 * 'f), 's1>) =
        fun state ->
            let lastFeed, lastMSstate, lastFState =
                match state with
                | None -> None, None, None
                | Some { mine = mine; inner = inner } ->
                    match inner with
                    | None -> mine, None, None
                    | Some v -> mine, Some v.currState, v.subState
            let mgen = m lastFeed
            match (asFunc mgen) lastMSstate with
            | Value ((mres, mfeed), mstate) ->
                // TODO: mf is discarded - that sound ok
                let fgen = f mres
                match (asFunc fgen) lastFState with
                | Value ((fres, ffeed), fstate) ->
                    Value (
                        fres, 
                        { mine = ffeed
                          inner = Some { currState = mstate
                                         subState = Some fstate } }
                    )
                | _ -> failwith "TODO"
            | _ -> failwith "TODO"
        |> create

    let feedback
        (seed: 'workingState)
        (f: 'workingState -> Gen<'output * 'workingState, 'innerState>)
        : Gen<'output, FeedbackState<'workingState, 'innerState>>
        =
        fun (state: FeedbackState<'workingState, 'innerState> option) ->
            let feedbackState, innerState =
                match state with
                | None -> seed, None
                | Some { mine = mine; inner = inner } -> mine, inner
            match asFunc (f feedbackState) innerState with
            | Value ((resF, feedStateF), innerStateF) ->
                Value (resF, { mine = feedStateF; inner = Some innerStateF })
            | Discard (Some innerStateF) -> 
                Discard (Some { mine = seed; inner = Some innerStateF })
            | Discard None -> Discard None
            | Stop -> Stop
        |> create
        
    let zero () =
        fun _ -> Discard None
        |> create

    let ofResult x =
        fun _ -> x
        |> create

    let ofValue x =
        fun _ -> Value (x, ())
        |> create
    
    /// Transforms a generator function to an effect function.    
    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> ofSeed2 (fun enumerator ->
            match enumerator.MoveNext() with
            | true -> Value (enumerator.Current, enumerator)
            | false -> Stop
        )
        
    let ofList (l: list<_>) =
        l
        |> ofSeed2 (fun l ->
            match l with
            | x::xs -> Value (x, xs)
            | [] -> Stop
        )

    // TODO: other builder methods
    type GenBuilder() =
        member _.Bind(m, f) = bind f m
        member _.Return(x: Res<GenResult<'v, unit>>) = match x with | Res x -> ofResult x
        member this.Yield(x) = this.Return(x)
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = x
        member _.Zero() = zero ()
        //member this.Delay(f) = f
        //member this.Run(f) = f ()
        //member this.While (guard, body) =
            //if not (guard()) 
            //then this.Zero() 
            //else
            //    this.Bind(body(), fun () ->
            //        this.While (guard, body))  
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
        member _.For (sequence: seq<'a>, body: 'a -> Gen<'b, _>) =
            let genSeq = ofSeq sequence 
            genSeq |> bind body

    type FeedbackBuilder() =
        inherit GenBuilder()
        member this.Bind(m, f) = bind f m
        member this.Bind(m, f) = bindFdb f m
        member this.Return(x) = ofResult x
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()
    
    
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
            match (asFunc x) state with
            | Value (x', state) -> Value (projection x', state)
            | Discard s -> Discard s
            | Stop -> Stop
        |> create

    let apply (xGen: Gen<'o1, _>) (fGen: Gen<'o1 -> 'o2, _>) : Gen<'o2, _> =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return Res.value result
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
    let (>=>) f g = Gen.pipeFx g f

    /// Kleisli "pipe" operator (gen >> fx)
    let (|=>) f g = Gen.pipe g f

    /// Bind operator
    let (>>=) m f = Gen.bind f m

let gen = Gen.gen
let fdb = Gen.fdb
