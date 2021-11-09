namespace FsLocalState

// TODO
(*
    A hint for the type argument names:
        * 'o: This stands for "output"
*)

type Gen<'o,'s> =
    | Gen of ('s option -> 'o list)

// TODO: rename DiscardWith to Discard
// TODO: 'f and f (as function name in bind) can be distracting
[<RequireQualifiedAccess>]
type GenResult<'e, 'd> =
    | Emit of 'e
    | DiscardWith of 'd
    | Stop

// 'f bei Fdb sollte immer hinten stehen - als Erweiterung von Gen
type GenEmit<'v,'s> = GenEmit of 'v * 's
type GenDiscard<'s> = GenDiscard of 's
type FdbEmit<'v,'f,'s> = FdbEmit of 'v * 'f * 's
type FdbDiscard<'f,'s> = FdbDiscard of 'f option * 's

type GenForGen<'o,'s> = Gen<GenResult<GenEmit<'o,'s>, GenDiscard<'s>>, 's> 
type GenResultGen<'o,'s> = GenResult<GenEmit<'o,'s>, GenDiscard<'s>>

type GenForFdb<'o,'f,'s> = Gen<GenResult<FdbEmit<'o,'f,'s>, FdbDiscard<'f,'s>>, 's> 
type GenResultFdb<'o,'f,'s> = GenResult<FdbEmit<'o,'f,'s>, FdbDiscard<'f,'s>>

type Fx<'i,'o,'s> =
    'i -> Gen<'o,'s>

type Init<'f> =
    | Init of 'f

// TODO: rename currState -> mstate, subState: fstate
[<Struct>]
type GenState<'scurr, 'ssub, 'rem> =
    { mstate: 'scurr
      fstate: 'ssub option
      leftovers: 'rem list }


module Control =
    type Emit<'value> = Emit of 'value
    type Feedback<'value, 'feedback> = Feedback of 'value * 'feedback
    type DiscardWith<'state> = DiscardWith of 'state
    type Stop = Stop


module GenResult =
    let isStop result = match result with | GenResult.Stop -> true | _ -> false

    type AggregateResult<'o, 's> =
        { results: GenResultGen<'o, 's> list
          isStopped: bool
          finalState: 's option }

    let mapUntilStop mapping (results: GenResultGen<_,_> list) =
        // TODO: Implement a "UntilStopResult" that doesn't have 'Stop' as case and get rid of the failwith.
        let resultsTilStop, finalState =
            results
            |> Seq.takeWhile (isStop >> not)
            |> Seq.mapFold
                (fun _ res ->
                    let newState = 
                        match res with
                        | GenResult.Emit (GenEmit (_, s)) -> Some s
                        | GenResult.DiscardWith (GenDiscard s) -> Some s
                        | GenResult.Stop -> failwith "Stop is not supported."
                    mapping res, newState
                )
                None
            |> fun (results, state) -> results |> Seq.toList, state
        { results = resultsTilStop
          isStopped = results.Length > resultsTilStop.Length
          finalState = finalState }

    let takeUntilStop results = mapUntilStop id results

    let emittedValues (results: GenResultGen<_,_> list) =
        results
        |> List.choose (fun res ->
            match res with
            | GenResult.Emit (GenEmit (v, _)) -> Some v
            | _ -> None
        )


module Gen =
    
    let unwrap (gen: Gen<_,_>) = let (Gen b) = gen in b


    // --------
    // Gen creation
    // --------

    /// Single case DU constructor.
    let create f = Gen f
    let createGen f : GenForGen<_,_> = Gen f
    let createFdb f : GenForFdb<_,_,_> = Gen f

    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let createWithSeed f seed =
        fun s ->
            let state = Option.defaultValue seed s
            f state
        |> createGen

    let createWithSeed2 seed f =
        createWithSeed seed f

    
    // --------
    // bind
    // --------

    let internal bindGenXxxGen discard processResult createX f m
        =
        let evalmres mres lastFState leftovers =
            match mres with
            | GenResult.Emit (GenEmit (mres, mstate)) ->
                let fgen = f mres
                let fres = (unwrap fgen) lastFState
                match fres with
                | [] -> 
                    let state = { mstate = mstate; fstate = lastFState; leftovers = leftovers }
                    [ discard state ]
                | results ->
                    [ for res in results do yield processResult res mstate leftovers ]
            | GenResult.DiscardWith (GenDiscard stateM) ->
                let state = { mstate = stateM; fstate = lastFState; leftovers = leftovers }
                [ discard state ]
            | GenResult.Stop ->
                [ GenResult.Stop ]
        let rec evalm lastMState lastFState =
            match (unwrap m) lastMState with
            | res :: leftovers ->
                evalmres res lastFState leftovers
            | [] ->
                match lastMState with
                | Some lastStateM ->
                    let state = { mstate = lastStateM; fstate = lastFState; leftovers = [] }
                    [ discard state ]
                | None ->
                    []
        fun state ->
            let lastMState, lastFState, lastLeftovers =
                match state with
                | None -> None, None, []
                | Some v -> Some v.mstate, v.fstate, v.leftovers
            match lastLeftovers with
            | x :: xs -> evalmres x lastFState xs
            | [] -> evalm lastMState lastFState
        |> createX

    let bind
        (f: 'o1 -> GenForGen<'o2, 's2>)
        (m: GenForGen<'o1, 's1>)
        : GenForGen<'o2, GenState<'s1, 's2, GenResultGen<'o1, 's1>>>
        =
        let discard state = GenResult.DiscardWith (GenDiscard state)
        let processResult res mstate leftovers =
            match res with
            | GenResult.Emit (GenEmit (fres, fstate)) ->
                let state = { mstate = mstate; fstate = Some fstate; leftovers = leftovers }
                GenResult.Emit (GenEmit (fres, state))
            | GenResult.DiscardWith (GenDiscard fstate) -> 
                let state = { mstate = mstate; fstate = Some fstate; leftovers = leftovers }
                GenResult.DiscardWith (GenDiscard state)
            | GenResult.Stop ->
                GenResult.Stop
        bindGenXxxGen discard processResult createGen f m

    let internal bindGenFdbFdb
        (f: 'o1 -> GenForFdb<'o2,'f,'s2>)
        (m: GenForGen<'o1,'s1>)
        : GenForFdb<'o2,'f,_> // TODO: _
        =
        let discard state = GenResult.DiscardWith (FdbDiscard (None, state))
        let processResult res mstate leftovers =
            match res with
            | GenResult.Emit (FdbEmit (fres, ffeedback, fstate)) ->
                let state = { mstate = mstate; fstate = Some fstate; leftovers = leftovers }
                GenResult.Emit (FdbEmit (fres, ffeedback, state))
            | GenResult.DiscardWith (FdbDiscard (ffeedback, fstate)) -> 
                let state = { mstate = mstate; fstate = Some fstate; leftovers = leftovers }
                GenResult.DiscardWith (FdbDiscard (ffeedback, state))
            | GenResult.Stop -> 
                GenResult.Stop
        bindGenXxxGen discard processResult createFdb f m

    let internal bindInitFdbGen
        (f: 'f -> GenForFdb<'o,'f,'s>)
        (m: Init<'f>)
        : GenForGen<_,_>
        =
        fun state ->
            let lastFeed, lastFState =
                match state with
                | None -> let (Init m) = m in m, None
                | Some v  -> v.mstate, v.fstate
            [ for res in (unwrap (f lastFeed)) lastFState do
                match res with
                | GenResult.Emit (FdbEmit (fvalue, feedback, fstate)) ->
                    let state = { mstate = feedback; fstate = Some fstate; leftovers = [] }
                    GenResult.Emit (GenEmit (fvalue, state))
                | GenResult.DiscardWith (FdbDiscard (feedback, fstate)) ->
                    let feedback =
                        match feedback with
                        | Some feedback -> feedback
                        | None -> lastFeed
                    let state = { mstate = feedback; fstate = Some fstate; leftovers = [] }
                    GenResult.DiscardWith (GenDiscard state)
                | GenResult.Stop ->
                    GenResult.Stop
            ]
        |> createGen


    // --------
    // return / yield
    // --------

    let ofValueOnce value =
        fun state -> 
            [
                GenResult.Emit (GenEmit (value, ()))
                GenResult.Stop 
            ]
        |> createGen

    let ofValueRepeating value : Gen<_,_> =
        create (fun _ -> [ value ])

    let returnValue<'v> (value: 'v) : GenForGen<'v, unit> =
        GenResult.Emit (GenEmit (value, ())) |> ofValueRepeating
    let returnValueThenStop (value: 'v) : GenForGen<'v, unit> =
        ofValueOnce value
    let returnDiscardWith<'v, 's> (state: 's) : GenForGen<'v,'s> =
        GenResult.DiscardWith (GenDiscard state) |> ofValueRepeating
    let returnStop<'v,'s> : GenForGen<'v,'s> =
        GenResult.Stop |> ofValueRepeating
    let returnFeedbackStop<'v,'f,'s> : GenForFdb<'v,'f,'s> =
        GenResult.Stop |> ofValueRepeating
    let returnFeedback<'discard, 'v, 'f, 'si> (value: 'v) (feedback: 'f) : GenForFdb<'v, 'f, unit> =
        GenResult.Emit (FdbEmit (value, feedback, ())) |> ofValueRepeating
    let returnFeedbackDiscardWith<'v, 'f> (feedback: 'f) : GenForFdb<'v, 'f, unit>  =
        GenResult.DiscardWith (FdbDiscard (Some feedback, ())) |> ofValueRepeating


    // --------
    // singleton / seq / list
    // --------

    let ofSeq (s: seq<_>) =
        fun enumerator ->
            let enumerator = enumerator |> Option.defaultWith (fun () -> s.GetEnumerator())
            [
                match enumerator.MoveNext() with
                | true -> GenResult.Emit (GenEmit (enumerator.Current, enumerator))
                | false -> GenResult.Stop
            ]
        |> createGen
        
    let ofList (list: list<_>) =
        fun l ->
            let l = l |> Option.defaultValue list
            [
                match l with
                | x::xs -> GenResult.Emit (GenEmit (x, xs))
                | [] -> GenResult.Stop
            ]
        |> createGen


    // --------
    // combine
    // --------

    type CombineInfo<'sa, 'sb> =
        { astate: 'sa option
          bstate: 'sb option }

    let combine 
        (a: GenForGen<'o, 'sa>)
        (b: unit -> GenForGen<'o, 'sb>)
        =
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
                        | GenResult.Emit (GenEmit (va, sa)) ->
                            astate <- Some sa
                            yield GenResult.Emit (GenEmit (va, { astate = astate; bstate = None }))
                        | GenResult.DiscardWith (GenDiscard sa) -> 
                            astate <- Some sa
                            yield GenResult.DiscardWith (GenDiscard { astate = astate; bstate = None })
                        | GenResult.Stop ->
                            isRunning <- false
                            yield GenResult.Stop
                if isRunning then
                    for res in getValue (b ()) state.bstate do
                        if isRunning then
                            match res with
                            | GenResult.Emit (GenEmit (vb, sb)) ->
                                yield GenResult.Emit (GenEmit (vb, { astate = astate; bstate = Some sb }))
                            | GenResult.DiscardWith (GenDiscard sb) -> 
                                yield GenResult.DiscardWith (GenDiscard { astate = astate; bstate = Some sb })
                            | GenResult.Stop ->
                                isRunning <- false
                                yield GenResult.Stop
            ]
        |> createGen


    // --------
    // Builder
    // --------

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = ofList x
        member _.Zero() = Gen (fun _ -> [])
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
        member _.Bind(m, f) = bindInitFdbGen f m
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindGenFdbFdb f m
        // returns
        member _.Return(Control.Feedback (value, feedback)) = returnFeedback value feedback
        member _.Return(Control.DiscardWith state) = returnFeedbackDiscardWith state
        member _.Return(Control.Stop) = returnFeedbackStop
    
    let gen = GenBuilder()
    let fdb = FeedbackBuilder()


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


[<RequireQualifiedAccess>]
module Arithmetic =
    let inline binOpBoth left right f =
        Gen.gen {
            let! l = left
            let! r = right
            return Control.Emit (f l r)
        }
    
    let inline binOpLeft left right f =
        Gen.gen {
            let l = left
            let! r = right
            return Control.Emit (f l r)
        }
    
    let inline binOpRight left right f =
        Gen.gen {
            let! l = left
            let r = right
            return Control.Emit (f l r)
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

    let gen = Gen.gen
    let fdb = Gen.fdb
