namespace FsLocalState

type Init<'f> =
    | Init of 'f

[<RequireQualifiedAccess>]
type GenResult<'v, 's> =
    | Emit of 'v * 's
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
    type Feedback<'value, 'feedback> = Feedback of 'value * 'feedback
    type DiscardWith<'state> = DiscardWith of 'state
    type Stop = Stop


module Gen =
    
    let unwrap gen = let (Gen b) = gen in b


    // --------
    // Helper
    // --------

    [<RequireQualifiedAccess>]
    type MapResult<'v, 's> =
        | Emit of 'v * 's
        | DiscardWith of 's

    // TODO: seems we don't need that
    ////let mapUntilStop mapping results =
    ////    results 
    ////    |> List.takeWhile (fun res ->
    ////        match res with
    ////        | GenResult.Stop -> false
    ////        | _ -> true
    ////    )
    ////    |> List.map (fun res ->
    ////        match res with
    ////        | GenResult.Emit (v, s) -> MapResult.Emit (v, s) |> mapping
    ////        | GenResult.DiscardWith s -> MapResult.DiscardWith s |> mapping
    ////        | GenResult.Stop -> GenResult.Stop // this can't happen
    ////    )


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
            let fres = (unwrap fgen) lastFState
            match fres with
            | [] -> [ GenResult.DiscardWith { currState = mstate; subState = lastFState; remaining = remaining } ]
            | results ->
                [ for res in results do
                    match res with
                    | GenResult.Emit (fres, fstate) ->
                        yield GenResult.Emit (fres, { currState = mstate; subState = Some fstate; remaining = remaining })
                    | GenResult.DiscardWith fstate -> 
                        yield GenResult.DiscardWith { currState = mstate; subState = Some fstate; remaining = remaining }
                    | GenResult.Stop -> 
                        yield GenResult.Stop
                ]
        let evalmres mres lastFState remaining =
            match mres with
            | GenResult.Emit (mres, mstate) ->
                let fgen = f mres
                evalf fgen mstate lastFState remaining
            | GenResult.DiscardWith stateM ->
               [ GenResult.DiscardWith { currState = stateM; subState = lastFState; remaining = remaining } ]
            | GenResult.Stop ->
                [ GenResult.Stop ]
        let rec evalm lastMState lastFState =
            match (unwrap m) lastMState with
            | res :: remaining -> evalmres res lastFState remaining
            | [] ->
                match lastMState with
                | Some lastStateM ->
                    [ GenResult.DiscardWith { currState = lastStateM; subState = lastFState; remaining = [] } ]
                | None ->
                    []
        fun state ->
            let lastMState, lastFState, remaining =
                match state with
                | None -> None, None, []
                | Some v -> Some v.currState, v.subState, v.remaining
            match remaining with
            | x :: xs -> evalmres x lastFState xs
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
                | GenResult.Stop ->
                    GenResult.Stop
            ]
        |> create


    // --------
    // return / yield
    // --------

    let ofValueOnce value =
        fun state -> 
            [
                GenResult.Emit(value, ())
                GenResult.Stop 
            ]
        |> create

    let ofValueRepeating (value: 'a) : Gen<_,_> =
        create (fun _ -> [ value ])

    let returnValue value : Gen<_,_> =
        GenResult.Emit(value, ()) |> ofValueRepeating
    let returnValueThenStop value : Gen<_,_> =
        ofValueOnce value
    let returnFeedback value feedback =
        GenResult.Emit(value, feedback) |> ofValueRepeating
    let returnDiscardWith<'a, 's, 'c> (state: 's) : Gen<GenResult<'a, 's>, 'c> =
        GenResult.DiscardWith state |> ofValueRepeating
    let returnStop<'a, 'b, 'c> : Gen<GenResult<'a, 'b>, 'c> =
        GenResult.Stop |> ofValueRepeating


    // --------
    // singleton / seq / list
    // --------

    let ofSeq (s: seq<_>) =
        s.GetEnumerator()
        |> createWithSeed2 (fun enumerator ->
            [
                match enumerator.MoveNext() with
                | true -> GenResult.Emit (enumerator.Current, enumerator)
                | false -> GenResult.Stop
            ]
        )
        
    let ofList (list: list<_>) =
        list
        |> createWithSeed2 (fun l ->
            [
                match l with
                | x::xs -> GenResult.Emit (x, xs)
                | [] -> GenResult.Stop
            ]
        )


    // --------
    // combine
    // --------

    type CombineInfo<'sa, 'sb> =
        { astate: 'sa option
          bstate: 'sb option }

    let combine (a: Gen<GenResult<'o, 'sa>, 'sa>) (b: unit -> Gen<GenResult<'o, 'sb>, 'sb>) =
        let getValue g state = (unwrap g) state
        fun state ->
            [
                let state =  state |> Option.defaultValue { astate = None; bstate = None }
                
                let mutable astate = state.astate
                let mutable isRunning = true

                // TODO: that looks quite crappy, buy maybe it's ok?
                // TODO: redundancy
                for res in getValue a state.astate do
                    if isRunning then
                        match res with
                        | GenResult.Emit (va, sa) ->
                            astate <- Some sa
                            yield GenResult.Emit (va, { astate = astate; bstate = None })
                        | GenResult.DiscardWith sa -> 
                            astate <- Some sa
                            yield GenResult.DiscardWith { astate = astate; bstate = None }
                        | GenResult.Stop ->
                            isRunning <- false
                            yield GenResult.Stop
                if isRunning then
                    for res in getValue (b ()) state.bstate do
                        if isRunning then
                            match res with
                            | GenResult.Emit (vb, sb) ->
                                yield GenResult.Emit (vb, { astate = astate; bstate = Some sb })
                            | GenResult.DiscardWith sb -> 
                                yield GenResult.DiscardWith { astate = astate; bstate = Some sb }
                            | GenResult.Stop ->
                                isRunning <- false
                                yield GenResult.Stop
            ]
        |> create


    // --------
    // Builder
    // --------

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = ofList x
        member _.Zero() = create (fun _ -> [])
        member _.For(sequence: seq<'a>, body) = ofSeq sequence |> bind body
        member _.Combine(x, delayed) = combine x delayed
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type GenBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        // returns
        member _.Return(Control.Emit value) = returnValue value
        member _.Return(Control.DiscardWith state) = returnDiscardWith state
        member _.Return(Control.Stop) = returnStop
        
    type FeedbackBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindFdb f m
        // returns
        member _.Return(Control.Feedback (value, feedback)) = returnFeedback value feedback
        member _.Return(Control.DiscardWith state) = returnDiscardWith state
        member _.Return(Control.Stop) = returnStop
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()


    // --------
    // map / apply / transformation
    // --------

    let mapValueAndState (proj: 'v -> 's -> 'o) (inputGen: Gen<_,_>) =
        fun state ->
            [
                for res in (unwrap inputGen) state do
                    match res with
                    | GenResult.Emit (v,s) -> GenResult.Emit (proj v s, s)
                    | GenResult.DiscardWith s -> GenResult.DiscardWith s
                    | GenResult.Stop -> GenResult.Stop
            ]
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
    // the 'comparison' constraint is a hack to prevent ambiguities in
    // F# operator overload resolution.

    // TODO: document operators and especially ==
    
    static member inline (+) (left: ^a when ^a: comparison, right) = Gen.binOpLeft left right (+)
    static member inline (-) (left: ^a when ^a: comparison, right) = Gen.binOpLeft left right (-)
    static member inline (*) (left: ^a when ^a: comparison, right) = Gen.binOpLeft left right (*)
    static member inline (/) (left: ^a when ^a: comparison, right) = Gen.binOpLeft left right (/)
    static member inline (%) (left: ^a when ^a: comparison, right) = Gen.binOpLeft left right (%)
    static member inline (==) (left: ^a when ^a: comparison, right) = Gen.binOpLeft left right (=)

    static member inline (+) (left, right: ^a when ^a: comparison) = Gen.binOpRight left right (+)
    static member inline (-) (left, right: ^a when ^a: comparison) = Gen.binOpRight left right (-)
    static member inline (*) (left, right: ^a when ^a: comparison) = Gen.binOpRight left right (*)
    static member inline (/) (left, right: ^a when ^a: comparison) = Gen.binOpRight left right (/)
    static member inline (%) (left, right: ^a when ^a: comparison) = Gen.binOpRight left right (%)
    static member inline (==) (left, right: ^a when ^a: comparison) = Gen.binOpRight left right (=)

    static member inline (+) (left, right) = Gen.binOpBoth left right (+)
    static member inline (-) (left, right) = Gen.binOpBoth left right (-)
    static member inline (*) (left, right) = Gen.binOpBoth left right (*)
    static member inline (/) (left, right) = Gen.binOpBoth left right (/)
    static member inline (%) (left, right) = Gen.binOpBoth left right (%)
    static member inline (==) (left, right) = Gen.binOpBoth left right (=)


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
