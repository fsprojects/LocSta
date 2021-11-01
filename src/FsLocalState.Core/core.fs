[<AutoOpen>]
module FsLocalState.Core

type InitResult<'f> =
    | Init of 'f

// TODO: Why are GenResult and FdbResult the same?

[<RequireQualifiedAccess>]
type GenResult<'o, 's> =
    | Value of 'o * 's
    | Discard
    | DiscardWith of 's
    | Stop

type Gen<'o, 's> =
    | Gen of ('s option -> 'o)

type Fx<'i, 'o, 's> =
    'i -> Gen<'o, 's>

// TODO: Why are GenState and FdbState the same?

[<Struct>]
type GenState<'sCurr, 'sSub> =
    { currState: 'sCurr
      subState: 'sSub option }

[<Struct>] 
type FdbState<'f, 's> = 
    { feedback: 'f
      inner: 's option }

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
                | Some { feedback = feedback; inner = inner } -> feedback, inner
            let fgen = f lastFeed
            match (unwrap fgen) lastFState with
            | GenResult.Value (fres, ffeed) ->
                GenResult.Value (fres, { feedback = ffeed; inner = None })
            | GenResult.DiscardWith ffeed ->
                GenResult.DiscardWith { feedback = ffeed; inner = None }
            | GenResult.Discard ->
                GenResult.DiscardWith { feedback = lastFeed; inner = lastFState }
            | GenResult.Stop ->
                GenResult.Stop
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

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = x
        member _.Zero() = ofValue GenResult.Discard
        member _.For (sequence: seq<'a>, body) = ofSeq sequence |> bind body
        // result ctors
        member _.value(v) = GenResult.Value (v, ())
        member _.feedback value feedback = GenResult.Value (value, feedback)
        member _.discard() : GenResult<'a, 's> = GenResult.Discard
        member _.discardWith(state) : GenResult<'a, 's> = GenResult.DiscardWith state
        member _.stop() : GenResult<'a, 's> = GenResult.Stop

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
