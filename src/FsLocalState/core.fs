
(*
A hint for the type argument names:
    * 'o: output of a Gen.
    * 'v: value; the actual value emitted by a Gen - usually corresponds to the output of a Gen.
    * 'f: feedback
    * 's: state
For the API surface, names like 'value or 'state are used instead of chars.
*)

// TODO: InlineIfLambda, ValueOptions, Structs
namespace FsLocalState

type Gen<'o,'s> = Gen of ('s option -> 'o)

// TODO: why is 's optional - what does it mean exactly? Could mean: Reset or UseLast.
//       Is it important to specify that or will it defined by the library?
// TODO: Is it really a good idea generaliziong Res instead of using disjoint results for Feed and Loop?
[<RequireQualifiedAccess>]
type Res<'v,'s> =
    | Continue of 'v list * 's
    | Stop of 'v list

[<RequireQualifiedAccess>]
type LoopState<'s> =
    | Update of 's
    | KeepLast
    | ResetDescendants
type LoopRes<'o,'s> = Res<'o, LoopState<'s>>
type LoopGen<'o,'s> = Gen<LoopRes<'o,'s>, 's>

[<Struct; RequireQualifiedAccess>]
type FeedType<'f> =
    | Update of 'f
    | KeepLast
    | ResetMe
    | ResetMeAndDescendants

type FeedState<'s,'f> = FeedState of 's option * FeedType<'f> option
type FeedRes<'o,'s,'f> = Res<'o, FeedState<'s,'f>>
type FeedGen<'o,'s,'f> = Gen<FeedRes<'o,'s,'f>, 's> 

[<Struct>]
type Init<'f> = Init of 'f

type Fx<'i,'o,'s> = 'i -> Gen<'o,'s>

[<Struct>]
type BindState<'sm, 'sk, 'm> =
    { mstate: 'sm option       // this is optional because m can have no state by just yielding Stop
      kstate: 'sk option       // means: k evaluates to a constant; "return" uses this.
      mleftovers: 'm list
      isStopped: bool }

/// Convenience for working directly with Gen funcs.
module Res =
    module Loop =
        let emit value state = Res.Continue ([value], LoopState.Update state)
        let emitAndKeepLast value = Res.Continue ([value], LoopState.KeepLast)
        let emitAndReset value = Res.Continue ([value], LoopState.ResetDescendants)
        let emitAndStop value = Res.Stop [value]
        let emitMany values state = Res.Continue (values, LoopState.Update state)
        let emitManyStateless values = Res.Continue (values, LoopState.KeepLast)
        let emitManyAndReset values = Res.Continue (values, LoopState.ResetDescendants)
        let emitManyAndStop values = Res.Stop values
        let skip state = Res.Continue ([], LoopState.Update state)
        let skipStateless = Res.Continue ([], LoopState.KeepLast)
        let skipAndReset = Res.Continue ([], LoopState.ResetDescendants)
        let stop = Res.Stop []

/// Vocabulary for Return of loop computations.
module Loop =
    type [<Struct>] Emit<'value> = Emit of 'value
    type [<Struct>] EmitAndReset<'value> = EmitAndReset of 'value
    type [<Struct>] EmitAndStop<'value> = EmitAndStop of 'value
    type [<Struct>] EmitMany<'value> = EmitMany of 'value list
    type [<Struct>] EmitManyAndReset<'value> = EmitManyAndReset of 'value list
    type [<Struct>] EmitManyAndStop<'value> = EmitManyAndStop of 'value list
    type [<Struct>] Skip = Skip
    type [<Struct>] SkipAndReset = SkipAndReset
    type [<Struct>] Stop = Stop

/// Vocabulary for Return of feed computations.
module Feed =
    type [<Struct>] Emit<'value, 'feedback> = Emit of 'value * 'feedback
    type [<Struct>] EmitMany<'value, 'feedback> = EmitMany of 'value list * 'feedback
    type [<Struct>] EmitAndStop<'value> = EmitAndStop of 'value
    type [<Struct>] EmitManyAndStop<'value> = EmitManyAndStop of 'value list
    type [<Struct>] SkipWith<'feedback> = SkipWith of 'feedback
    type [<Struct>] Stop = Stop
    // TODO: Will man Reset wirklich als Teil der Builder-Abstraktion?
    // TODO: 'value list oder 'value
    // TODO: So strukturieren wie bei Loop
    type [<Struct>] ResetMe = ResetMe
    type [<Struct>] ResetMeAndDescendants = ResetMeAndDescendants


// TODO: make some things internal and expose them explicitly via Gen type
[<AutoOpen>]
module Gen =

    let run (gen: Gen<_,_>) = let (Gen b) = gen in b


    // --------
    // Gen creation
    // --------

    let createGen f = Gen f
    let createLoop f : LoopGen<_,_> = Gen f
    let createFeed f : FeedGen<_,_,_> = Gen f

    
    // --------
    // Active Recognizers
    // --------

    let (|LoopStateToOption|) defaultState currState =
        match currState with
        | LoopState.Update s -> Some s
        | LoopState.KeepLast -> defaultState
        | LoopState.ResetDescendants -> None
    
    // --------
    // bind
    // --------

    // TODO: Bräuchte buildState nicht auch den lastState? Und as TODO von unten - welche Rolle spielt das?
    let internal bindLoopWhateverGen evalk buildSkip createWhatever m =
        fun state ->
            let evalmres mres lastMState lastKState isStopped =
                match mres with
                | Res.Continue (mval :: mleftovers, LoopStateToOption lastMState mstate) ->
                    evalk mval mstate mleftovers lastKState isStopped
                | Res.Continue ([], LoopStateToOption lastMState mstate) ->
                    let state = { mstate = mstate; kstate = lastKState; mleftovers = []; isStopped = isStopped }
                    // TODO: why "None" in case of Res.Continue?
                    Res.Continue ([], buildSkip state)
                | Res.Stop (mval :: mleftovers) ->
                    evalk mval lastMState mleftovers lastKState isStopped
                | Res.Stop [] ->
                    Res.Stop []
            match state with
            | Some { mstate = lastMState; mleftovers = x :: xs; kstate = lastKState; isStopped = isStopped } ->
                evalk x lastMState xs lastKState isStopped
            | Some { mleftovers = []; isStopped = true } ->
                Res.Stop []
            | Some { mstate = lastMState; mleftovers = []; kstate = kstate } ->
                evalmres (run m lastMState) lastMState kstate false
            | None ->
                evalmres (run m None) None None false
        |> createWhatever

    let bind
        (k: 'o1 -> LoopGen<'o2,'s2>)
        (m: LoopGen<'o1,'s1>)
        : LoopGen<'o2, BindState<'s1,'s2,'o1>>
        =
        let evalk mval mstate mleftovers lastKState isStopped =
            match run (k mval) lastKState with
            | Res.Continue (kvalues, kstate) ->
                let newState kstate = { mstate = mstate; kstate = kstate; mleftovers = mleftovers; isStopped = isStopped }
                match kstate with
                | LoopState.Update kstate ->
                    Res.Continue (kvalues, LoopState.Update (newState (Some kstate)))
                | LoopState.KeepLast ->
                    Res.Continue (kvalues, LoopState.Update (newState lastKState))
                | LoopState.ResetDescendants ->
                    Res.Continue (kvalues, LoopState.ResetDescendants)
            | Res.Stop kvalues ->
                Res.Stop kvalues
        let buildSkip state = LoopState.Update state
        bindLoopWhateverGen evalk buildSkip createLoop m

    let internal bindLoopFeedFeed
        (k: 'o1 -> FeedGen<'o2,'s2,'f>)
        (m: LoopGen<'o1,'s1>)
        : FeedGen<'o2,_,'f> // TODO: _
        =
        let evalk mval mstate mleftovers lastKState isStopped =
            match run (k mval) lastKState with
            | Res.Continue (kvalues, FeedState (kstate, feedState)) ->
                let state = { mstate = mstate; kstate = kstate; mleftovers = mleftovers; isStopped = isStopped }
                Res.Continue (kvalues, FeedState (Some state, feedState))
            | Res.Stop kvalues ->
                Res.Stop kvalues
        let buildSkip state = FeedState (Some state, None)
        bindLoopWhateverGen evalk buildSkip createFeed m

    let internal bindInitFeedLoop
        (k: 'f -> FeedGen<'o,'s,'f>)
        (m: Init<'f>)
        : LoopGen<_,_>
        =
        fun state ->
            let getInitial () = let (Init m) = m in m
            let evalk lastFeed lastKState =
                match run (k lastFeed) lastKState with
                | Res.Continue (kvalues, FeedState (kstate, Some feedback)) ->
                    let feedback,kstate =
                        match feedback with
                        | FeedType.Update feedback -> Some feedback, kstate
                        | FeedType.KeepLast -> Some lastFeed, kstate
                        | FeedType.ResetMe -> None, kstate
                        | FeedType.ResetMeAndDescendants -> None, None
                    let state = { mstate = feedback; kstate = kstate; mleftovers = []; isStopped = false }
                    Res.Continue (kvalues, LoopState.Update state)
                | Res.Continue (kvalues, FeedState (kstate, None)) ->
                    let state = { mstate = Some lastFeed; kstate = kstate; mleftovers = []; isStopped = false }
                    Res.Continue (kvalues, LoopState.Update state)
                | Res.Stop kvalues ->
                    Res.Stop kvalues
            match state with
            | Some { isStopped = true } ->
                Res.Stop []
            | None ->
                evalk (getInitial()) None
            | Some { mstate = None; kstate = kstate } ->
                evalk (getInitial()) kstate
            | Some { mstate = Some feedback; kstate = kstate } ->
                evalk feedback kstate
        |> createLoop


    // --------
    // create of values
    // --------

    module internal CreateInternal =
        let inline internal returnLoopRes res =
            (fun _ -> res) |> createLoop
        let inline internal returnFeedRes res =
            (fun _ -> res) |> createFeed

        let inline internal returnContinue<'v, 's> values state : LoopGen<'v,'s> =
            returnLoopRes (Res.Continue (values, state))
        let inline internal returnContinueValues<'v, 's> values : LoopGen<'v,'s> =
            returnLoopRes (Res.Continue (values, LoopState.KeepLast))
        let inline internal returnStop<'v,'s> values : LoopGen<'v,'s> =
            returnLoopRes (Res.Stop values)
        let inline internal returnResetDescendants<'v,'s> values : LoopGen<'v,'s> =
            returnLoopRes (Res.Continue (values, LoopState.ResetDescendants))
    
        let inline internal returnContinueFeed<'v,'s,'f> values feedback : FeedGen<'v,'s,'f> =
            returnFeedRes (Res.Continue (values, feedback))
        let inline internal returnContinueValuesFeed<'v,'s,'f> values feedback : FeedGen<'v,'s,'f> =
            returnFeedRes (Res.Continue (values, FeedState (None, Some (FeedType.Update feedback))))
        let inline internal returnStopFeed<'v,'s,'f> values : FeedGen<'v,'s,'f> =
            returnFeedRes (Res.Stop values)

    let ofRepeatingValues<'v, 's> values : LoopGen<'v,'s> =
        CreateInternal.returnLoopRes (Res.Continue (values, LoopState.KeepLast))
    let ofRepeatingValue<'v, 's> value : LoopGen<'v,'s> =
        ofRepeatingValues [value]
    let ofOneTimeValues<'v, 's> values : LoopGen<'v,'s> =
        CreateInternal.returnLoopRes (Res.Stop values)
    let ofOneTimeValue<'v, 's> value : LoopGen<'v,'s> =
        ofOneTimeValues [value]


    // --------
    // create of seq / list
    // --------

    // TODO: think about dropping ofSeq support completely
    let ofSeqOneByOne (s: seq<_>) =
        fun enumerator ->
            let enumerator = enumerator |> Option.defaultWith (fun () -> s.GetEnumerator())
            match enumerator.MoveNext() with
            | true -> Res.Loop.emit enumerator.Current enumerator
            | false -> Res.Loop.stop
        |> createLoop

    // TODO: Improve naming

    /// Emits the head of the list and retains the excess or stops on an empty list.
    let ofListOneByOne (list: list<_>) =
        fun l ->
            let l = l |> Option.defaultValue list
            match l with
            | x::xs -> Res.Loop.emit x xs
            | [] -> Res.Loop.stop
        |> createLoop

    /// Emits the complete list or stopps on an empty list.
    let ofListAllAtOnce (list: list<_>) =
        fun _ ->
            match list with
            | [] -> Res.Loop.stop
            | l -> Res.Loop.emitManyStateless l
        |> createLoop


    // --------
    // combine
    // --------

    type CombineInfo<'sa, 'sb> =
        { astate: 'sa option
          bstate: 'sb option }

    let internal combineLoop
        (a: LoopGen<'o, 'sa>)
        (b: unit -> LoopGen<'o, 'sb>)
        : LoopGen<'o, CombineInfo<'sa,'sb>>
        =
        fun state ->
            let state =  state |> Option.defaultValue { astate = None; bstate = None }
            let ares = run a state.astate
            match ares with
            | Res.Continue (avalues, LoopStateToOption state.astate astate) ->
                let bres = run (b()) state.bstate
                match bres with
                | Res.Continue (bvalues, LoopStateToOption state.bstate bstate) ->
                    Res.Continue (avalues @ bvalues, LoopState.Update { astate = astate; bstate = bstate })
                | Res.Stop bvalues ->
                    Res.Stop (avalues @ bvalues)
            | Res.Stop avalues ->
                Res.Stop avalues
        |> createLoop

    // TODO: Redundant with combine
    // TODO: verstehe ich noch nicht ganz: was passiert denn mit afeedback? -> Testen:
    // Wie genau verhält es sich, wenn ich 2 feeds combine (und 'fa, 'fb, 'fc)?
    let internal combineFeed
        (a: FeedGen<'o, 'sa, 'f>)
        (b: unit -> FeedGen<'o, 'sb, 'f>)
        : FeedGen<'o, CombineInfo<'sa,'sb>, 'f>
        =
        fun state ->
            let state =  state |> Option.defaultValue { astate = None; bstate = None }
            match run a state.astate with
            // TODO: document this: 'afeedback' is unused, which means: the last emitted feedback is used when combining
            | Res.Continue (avalues, FeedState (astate, afeedback)) ->
                match run (b()) state.bstate with
                | Res.Continue (bvalues, FeedState (bstate, bfeedback)) ->
                    let state = { astate = astate; bstate = bstate }
                    Res.Continue (avalues @ bvalues, FeedState (Some state, bfeedback))
                | Res.Stop bvalues ->
                    Res.Stop (avalues @ bvalues)
            | Res.Stop avalues ->
                Res.Stop avalues
        |> createFeed


    // -------
    // evaluation / transform to other domains
    // -------
    
    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let toSeq (g: LoopGen<_,'s>) : seq<_> =
        let f = run g
        let mutable state = None
        let mutable resume = true
        seq {
            while resume do
                match f state with
                | Res.Continue (values, LoopStateToOption state fstate) ->
                    state <- fstate
                    yield! values
                | Res.Stop values ->
                    resume <- false
                    yield! values
        }

    // TODO: quite redundant with toSeq, but wrapping it's 'g' seems inefficient
    let toSeqFx (fx: 'i -> LoopGen<'o,'s>) : seq<'i> -> seq<'o> =
        let mutable state = None
        let mutable resume = true
        fun inputValues ->
            let enumerator = inputValues.GetEnumerator()
            seq {
                while resume && enumerator.MoveNext() do
                    match run (fx enumerator.Current) state with
                    | Res.Continue (values, LoopStateToOption state fstate) ->
                        state <- fstate
                        yield! values
                    | Res.Stop values ->
                        resume <- false
                        yield! values
            }
    
    let toList gen =
        toSeq gen |> Seq.toList

    let toListn count gen =
        toSeq gen |> Seq.truncate count |> Seq.toList

    let toListFx fx input =
        input |> toSeqFx fx |> Seq.toList

    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen


    // --------
    // Builder
    // --------

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = ofListAllAtOnce x
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()
        member _.For(list: list<_>, body) = list |> toListFx body |> ofListAllAtOnce

    type LoopBuilder() =
        inherit BaseBuilder()
        member _.Zero() = CreateInternal.returnContinue [] (LoopState.KeepLast)
        member _.Bind(m, f) = bind f m
        // TODO
        //member _.For(sequence: seq<'a>, body) = ofSeq sequence |> onStopThenSkip |> bind body
        member _.Combine(x, delayed) = combineLoop x delayed
        // returns
        // TODO: Die müssen alle in coreLoopTests abgetestet sein
        member _.Yield(value) : LoopGen<_,_> = CreateInternal.returnContinueValues [value]
        member _.Return(Loop.Emit value) = CreateInternal.returnContinueValues [value]
        member _.Return(Loop.EmitAndReset value) = CreateInternal.returnResetDescendants [value]
        member _.Return(Loop.EmitAndStop value) = CreateInternal.returnStop [value]
        member _.Return(Loop.EmitMany values) = CreateInternal.returnContinueValues values
        member _.Return(Loop.EmitManyAndReset values) = CreateInternal.returnResetDescendants values
        member _.Return(Loop.EmitManyAndStop values) = CreateInternal.returnStop values
        member _.Return(Loop.Skip) = CreateInternal.returnContinueValues []
        member _.Return(Loop.SkipAndReset) = CreateInternal.returnResetDescendants []
        member _.Return(Loop.Stop) = CreateInternal.returnStop []
        
    type FeedBuilder() =
        inherit BaseBuilder()
        member _.Zero() = CreateInternal.returnContinueFeed [] (FeedState (None,None))
        member _.Bind(m, f) = bindInitFeedLoop f m
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindLoopFeedFeed f m
        //member _.For(list: list<'a>, body) = ofListAllAtOnce list |> onStopThenSkip |> bindLoopFeedFeed body
        // TODO
        //member _.For(sequence: seq<'a>, body) = ofSeq sequence |> onStopThenSkip |> bindLoopFeedFeed body
        member _.Combine(x, delayed) = combineLoop x delayed
        member _.Combine(x, delayed) = combineFeed x delayed
        // returns
        // TODO: Die müssen alle in coreLoopTests abgetestet sein
        member _.Yield(value, feedback) = CreateInternal.returnContinueValuesFeed [value] feedback
        member _.Return(Feed.Emit (value, feedback)) = CreateInternal.returnContinueValuesFeed [value] feedback
        member _.Return(Feed.EmitMany (values, feedback)) = CreateInternal.returnContinueValuesFeed values feedback
        member _.Return(Feed.EmitAndStop value) = CreateInternal.returnStopFeed [value]
        member _.Return(Feed.EmitManyAndStop values) = CreateInternal.returnStopFeed values
        member _.Return(Feed.SkipWith feedback) = CreateInternal.returnContinueValuesFeed [] feedback
        member _.Return(Feed.Stop) = CreateInternal.returnStopFeed []
        // TODO (siehe Kommentar oben)
        member _.Return(Feed.ResetMe) = CreateInternal.returnContinueFeed [] (FeedState (None, Some FeedType.ResetMe))
        // TODO (siehe Kommentar oben)
        member _.Return(Feed.ResetMeAndDescendants) =
            CreateInternal.returnContinueFeed [] (FeedState (None, Some FeedType.ResetMeAndDescendants))
    
    let loop = LoopBuilder()
    let feed = FeedBuilder()


    // -------
    // Kleisli composition
    // -------

    let pipe (g: Fx<_,_,_>) (f: Gen<_,_>) : Gen<_,_> =
        loop {
            let! f' = f
            return! g f' 
        }

    let pipeFx (g: Fx<_,_,_>) (f: Fx<_,_,_>): Fx<_,_,_> =
        fun x -> loop {
            let! f' = f x
            return! g f' 
        }

    
    // --------
    // map / apply / transformation
    // --------

    let map2 (proj: 'v -> LoopState<'s> option -> 'o) (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            let mapValues values state = [ for v in values do proj v state ]
            match run inputGen state with
            | Res.Continue (values, state) ->
                Res.Continue (mapValues values (Some state), state)
            | Res.Stop values ->
                Res.Stop (mapValues values None)
        |> createLoop

    let map proj (inputGen: LoopGen<_,_>) =
        map2 (fun v _ -> proj v) inputGen

    let apply xGen fGen =
        loop {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            yield result
        }

    
    // -------
    // count
    // -------
    
    let inline count inclusiveStart increment =
        feed {
            let! curr = Init inclusiveStart
            yield curr, curr + increment
        }

    let inline countToAndThen inclusiveStart increment inclusiveEnd onEnd =
        loop {
            let! c = count inclusiveStart increment
            match c <= inclusiveEnd with
            | true -> yield c
            | false -> return! onEnd
        }

    let inline countTo inclusiveStart increment inclusiveEnd =
        countToAndThen inclusiveStart increment inclusiveEnd (CreateInternal.returnStop [])
    
    let inline countToCyclic inclusiveStart increment inclusiveEnd =
        countToAndThen inclusiveStart increment inclusiveEnd (CreateInternal.returnResetDescendants [])


    // --------
    // onStop trigger
    // --------

    type OnStopThenState<'s> =
        | RunInput of 's option
        | UseDefault

    let inline internal onStopThenValues defaultValues (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            let state = state |> Option.defaultValue (RunInput None)
            match state with
            | UseDefault ->
                Res.Continue (defaultValues, LoopState.Update UseDefault)
            | RunInput state ->
                let continueWith values state = Res.Loop.emitMany values (RunInput state)
                match run inputGen state with
                | Res.Continue (values, LoopStateToOption None state) ->
                    continueWith values state
                | Res.Stop values ->
                    Res.Loop.emitMany values UseDefault
        |> createLoop
        
    let inline onStopThenDefault defaultValue (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenValues [defaultValue] inputGen

    let inline onStopThenSkip (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenValues [] inputGen


[<RequireQualifiedAccess>]
module Arithmetic =
    let inline binOpBoth left right f =
        Gen.loop {
            let! l = left
            let! r = right
            yield f l r
        }
    
    let inline binOpLeft left right f =
        Gen.loop {
            let l = left
            let! r = right
            yield f l r
        }
    
    let inline binOpRight left right f =
        Gen.loop {
            let! l = left
            let r = right
            yield f l r
        }


type Gen<'o,'s> with
    // the 'comparison' constraint is a hack to prevent ambiguities in
    // F# operator overload resolution.

    // TODO: document operators and especially ==
    
    static member inline (+) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (+)
    static member inline (-) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (-)
    static member inline (*) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (*)
    static member inline (/) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (/)
    static member inline (%) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (%)
    static member inline (==) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (=)

    static member inline (+) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (+)
    static member inline (-) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (-)
    static member inline (*) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (*)
    static member inline (/) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (/)
    static member inline (%) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (%)
    static member inline (==) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (=)

    static member inline (+) (left, right) = Arithmetic.binOpBoth left right (+)
    static member inline (-) (left, right) = Arithmetic.binOpBoth left right (-)
    static member inline (*) (left, right) = Arithmetic.binOpBoth left right (*)
    static member inline (/) (left, right) = Arithmetic.binOpBoth left right (/)
    static member inline (%) (left, right) = Arithmetic.binOpBoth left right (%)
    static member inline (==) (left, right) = Arithmetic.binOpBoth left right (=)


[<AutoOpen>]
module TopLevelOperators =

    /// Kleisli operator (fx >> fx)
    let (>=>) f g = Gen.pipeFx g f

    /// Kleisli "pipe" operator (gen >> fx)
    let (|=>) f g = Gen.pipe g f

    /// Bind operator
    let (>>=) m f = Gen.bind f m

    let loop = Gen.loop
    let feed = Gen.feed
