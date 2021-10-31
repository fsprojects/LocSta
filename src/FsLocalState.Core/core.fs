[<AutoOpen>]
module FsLocalState.Core

type BaseResult<'o, 's> =
    | Value of 'o * 's
    | Discard of 's option
    | Stop

type FdbResult<'o, 's> =
    | FdbResult of BaseResult<'o, 's>

type GenFunc<'o, 's> =
    's option -> BaseResult<'o, 's>

type Gen<'o, 's> =
    | Gen of GenFunc<'o, 's>

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
    
    /// Wraps a BaseResult into a gen.
    let ofResult (x: BaseResult<_,_>) : Gen<_,_> = create (fun _ -> x)

    /// Wraps an arbitrary value into a gen.
    let ofValue x = create (fun _ -> Value (x, ()))

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
            match (unwrap m) lastMState with
            | Value (mres, mstate) ->
                let fGen = f mres
                match (unwrap fGen) lastFState with
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
            let lastFeed, lastMState, lastFState =
                match state with
                | None -> None, None, None
                | Some { feedback = mine; inner = inner } ->
                    match inner with
                    | None -> mine, None, None
                    | Some v -> mine, Some v.currState, v.subState
            let mgen = m lastFeed
            match (unwrap mgen) lastMState with
            | Value ((mres, mfeed), mstate) ->
                // TODO: mf is discarded - that sound ok
                let fgen = f mres
                match (unwrap fgen) lastFState with
                | Value ((fres, ffeed), fstate) ->
                    Value (
                        fres, 
                        { feedback = ffeed
                          inner = Some { currState = mstate
                                         subState = Some fstate } }
                    )
                | Discard (Some fstate) ->
                    Discard (
                        Some  { feedback = lastFeed
                                inner = Some { currState = mstate
                                               subState = Some fstate } }
                    )
                | Discard None ->
                    Discard (
                        Some  { feedback = lastFeed
                                inner = Some { currState = mstate
                                               subState = lastFState } }
                    )
                | Stop -> Stop
            | Discard (Some mstate) ->
                Discard (
                    Some  { feedback = lastFeed
                            inner = Some { currState = mstate
                                           subState = lastFState } }
                )
            | Discard None ->
                match lastMState with
                | Some lastMState ->
                    Discard (
                        Some  { feedback = lastFeed
                                inner = Some { currState = lastMState
                                               subState = lastFState } }
                    )
                | None ->
                    Discard None
            | Stop -> Stop
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
    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = x
        member _.Zero() = ofResult (Discard None)
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

    // TODO: other builder methods
    type GenBuilder() =
        inherit BaseBuilder()
        
        // builder methods
        member _.Bind(m, f) = bind f m 
        member _.Return(x) = ofResult x
        member this.Yield(x) = this.Return(x)
        
        // result ctors
        member _.value(v) = Value (v, ())
        member _.discard<'a, 'b>() = Discard None
        member _.discard(state) = Discard (Some state)
        member _.stop<'a, 'b>() = Stop

    type FeedbackBuilder() =
        inherit BaseBuilder()
        
        // builder methods
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindFdb f m
        member _.Return(x) = match x with | FdbResult x -> ofResult x
        member this.Yield(x) = this.Return(x)
        
        // result ctors
        member _.value value feedback = FdbResult (Value ((value, Some feedback), ()))
        member _.discard<'a, 'b>() = FdbResult (Discard None)
        member _.discard(state) = FdbResult (Discard (Some state))
        member _.stop<'a, 'b>() = FdbResult Stop
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()
    

    // --------
    // feedback
    // --------

    let inline init seed =
        fun feedback -> gen {
            let feedback = feedback |> Option.defaultValue seed
            return Value ((feedback, feedback), ())
        }
    

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
            match (unwrap x) state with
            | Value (x', state) -> Value (projection x', state)
            | Discard s -> Discard s
            | Stop -> Stop
        |> create

    let apply (xGen: Gen<'o1, _>) (fGen: Gen<'o1 -> 'o2, _>) : Gen<'o2, _> =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return gen.value result
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
module Globals =

    /// Kleisli operator (fx >> fx)
    let (>=>) f g = Gen.pipeFx g f

    /// Kleisli "pipe" operator (gen >> fx)
    let (|=>) f g = Gen.pipe g f

    /// Bind operator
    let (>>=) m f = Gen.bind f m

    let gen = Gen.gen
    let fdb = Gen.fdb
    let init = Gen.init
