[<AutoOpen>]
module FsLocalState.Core

type GenFunc<'output, 'state, 'reader> =
    'state option -> 'reader -> ('output * 'state) option

type Gen<'output, 'state, 'reader> =
    | Gen of GenFunc<'output, 'state, 'reader>

// TODO: seems to be impossible having a single case DU here?
type Eff<'input, 'output, 'state, 'reader> =
    'input -> Gen<'output, 'state, 'reader>

[<Struct>]
type StateAcc<'a, 'b> = { currState: 'a; subState: 'b }

module Gen =

    // --------
    // Construction / Init
    // --------

    let create f = Gen f
    
    let initValue seed f =
        fun s r ->
            let state = Option.defaultValue seed s
            f state r
        |> create

    let initWith seedFunc f =
        fun s r ->
            let state = Option.defaultWith seedFunc s
            f state r
        |> create

    // -----
    // Monad
    // -----

    let run gen =
        let (Gen b) = gen in b

    let bind (f: 'o1 -> Gen<'o2, 's2, 'r>) (m: Gen<'o1, 's1, 'r>) : Gen<'o2, StateAcc<'s1, 's2>, 'r> =
        fun (s: StateAcc<'s1, 's2> option) r ->
            let unpackedLocalState =
                match s with
                | None -> { currState = None; subState = None }
                | Some v -> { currState = Some v.currState; subState = Some v.subState }
            match (run m) unpackedLocalState.currState r with
            | Some m' ->
                let fGen = fst m' |> f
                match (run fGen) unpackedLocalState.subState r with
                | Some f' -> Some (fst f', { currState = snd m'; subState = snd f' })
                | None -> None
            | None -> None
        |> create

    let ofValue x =
        fun _ _ -> Some (x, ())
        |> create

    let ofFactory factory =
        fun s _ ->
            let instance = Option.defaultWith factory s
            Some (instance, instance)
        |> create

    let zero () =
        fun _ _ -> None
        |> Gen

    // TODO: Who really needs that?
    //// TODO: Docu
    //// TODO: other builder methods
    //type GenBuilderEx<'a>() =
    //    member _.Bind(m: Gen<_, _, 'a>, f) = bind m f
    //    member _.Return x = ret x 
    //    member _.ReturnFrom x = x
    //    member _.Zero () = zero ()

    // TODO: other builder methods
    type GenBuilder<'r>() =
        member _.Bind (m: Gen<_, _, 'r>, f: _ -> Gen<_, _, 'r>) = bind f m
        member _.Return x = ofValue x
        member _.ReturnFrom x = x
        member _.Zero () = zero ()

    let gen<'a> = GenBuilder<'a>()
    let genu = GenBuilder<Unit>()
    

    // --------
    // map / apply
    // --------

    let map projection x =
        gen {
            match! x with
            | Some res -> return projection res
            | None -> ()
        }

    let apply (xGen: Gen<'v1, _, 'r>) (fGen: Gen<'v1 -> 'v2, _, 'r>) : Gen<'v2, _, 'r> =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return result
        }


    // -------
    // Kleisli
    // -------

    let kleisli (g: Eff<'a, 'b, _, _>) (f: Gen<'a, _, _>) : Gen<'b, _, _> =
        gen {
            let! f' = f
            return! g f' 
        }

    
    // ------
    // Others
    // ------

    /// Reads the global state.
    let read () =
        fun _ r -> Some (r, ())
        |> create

    /// Transforms a generator function to an effect function.    
    let toEff (gen: Gen<'s, 'r, 'o>) : Eff<unit, 's, 'r, 'o> =
        fun () -> gen

    let feedback
        (seed: 'workingState)
        (f: 'workingState -> 'r -> Gen<'output * 'workingState, 'innerState, 'r>)
        : Gen<'output, 'workingState * 'innerState option, 'r>
        =
        fun (s: ('workingState * 'innerState option) option) r ->
            let feedbackState, innerState =
                match s with
                | None -> seed, None
                | Some (my, inner) -> my, inner
            match run (f feedbackState r) innerState r with
            | Some res ->
                let feed = fst res
                let innerState = snd res
                Some (fst feed, (snd feed, Some innerState))
            | None -> None
        |> create

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random<'a> () =
        fun _ (_: 'a) -> Some (dotnetRandom.NextDouble(), ())
        |> create

    let countFrom<'a> inclusiveStart increment =
        fun s (_: 'a) ->
            let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
            let newValue = state + increment
            Some (newValue, newValue)
        |> create

    let count0<'a> = countFrom<'a> 0 1
    
    // TODO: countFloat

    /// Delays a given value by 1 cycle.
    let delay seed input =
        feedback seed (fun state _ ->
            gen {
                return state, input
            }
        )
    
    /// Positive slope.
    let slopeP seed input =
        feedback seed (fun state _ ->
            gen {
                let res =
                    match state, input with
                    | false, true -> true
                    | _ -> false
                return res, input
            }
        )
    
    /// Negative slope.
    let slopeN seed input =
        feedback seed (fun state _ ->
            gen {
                let res =
                    match state, input with
                    | true, false -> true
                    | _ -> false
                return res, input
            }
        )
    
    // TODO
    // let toggle seed =
    //     let f p _ =
    //         match p with
    //         | true -> {value=0.0; state=false}
    //         | false -> {value=1.0; state=true}
    //     f |> liftSeed seed |> L

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

type Gen<'v, 's, 'r> with
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


module Operators =

    /// Feedback with reader state
    let (<|>) seed f = Gen.feedback seed f

    /// map operator
    let (<!>) gen projection = Gen.map projection gen

    /// apply operator
    let (<*>) fGen xGen = Gen.apply xGen fGen

    /// Kleisli operator (eff >> eff)
    let (>=>) f g = Gen.kleisli g f

    /// Kleisli "pipe" operator (gen >> eff)
    let (|=>) f g = Gen.kleisli g f

let gen<'a> = Gen.gen<'a>
let genu = Gen.genu

//let defState sfeed = Gen.feedback seed (fun fdbState _ ->
//    fun s _ ->
//        // only mutable within a single evaluation, so still persistent overall state
//        let muftable x = fdbState
//        let s = s |> Option.defaultValue seed
//        let getterAndSetter =
//            {|
//                get = x
//                set = fun value ->
//                    x <- value
//            |}
//        Some ((getterAndSetter, fdbState), s)
//    |> Gen.create
//)

//let res =
//    genu {
//        let! stateDef = defState 0
//        let! state = stateDef.get
//        return ($"State: %d{state}")
//    }


//type FeedbackBuilder<'a>(seed: 'a) =
//    member _.Return x = Gen.ofValue x
//    member _.ReturnFrom x = x
//    member _.Zero () = Gen.zero ()
//    member _.Bind 
//        (m: Gen<'o1, 's1, 'r>,
//         f: 'o1 -> Gen<'o2, 's2, 'r>) 
//        : Gen<'o2, StateAcc<'s1, 's2>, 'r> 
//        =
//        fun (s: StateAcc<'s1, 's2> option) r ->
//            Gen.feedback seed (fun fdbState _ ->
//                fun s' r ->
//                    let unpackedLocalState =
//                        match s with
//                        | None -> { currState = None; subState = None }
//                        | Some v -> { currState = Some v.currState; subState = Some v.subState }
//                    match (Gen.run m) unpackedLocalState.currState r with
//                    | Some m' ->
//                        let fGen = fst m' |> f
//                        match (Gen.run fGen) unpackedLocalState.subState r with
//                        | Some f' -> Some (fst f', { currState = snd m'; subState = snd f' })
//                        | None -> None
//                    | None -> None
//                |> Gen.create
//            )
//            //let unpackedLocalState =
//            //    match s with
//            //    | None -> { currState = None; subState = None }
//            //    | Some v -> { currState = Some v.currState; subState = Some v.subState }
//            //match (Gen.run m) unpackedLocalState.currState r with
//            //| Some m' ->
//            //    let fGen = fst m' |> f
//            //    match (Gen.run fGen) unpackedLocalState.subState r with
//            //    | Some f' -> Some (fst f', { currState = snd m'; subState = snd f' })
//            //    | None -> None
//            //| None -> None
//        |> Gen.create
//        //let feedback
//        //    (seed: 'workingState)
//        //    (f: 'workingState -> 'r -> Gen<'output * 'workingState, 'innerState, 'r>)
//        //    : Gen<'output, 'workingState * 'innerState option, 'r>
//        //    =
//        //    fun (s: ('workingState * 'innerState option) option) r ->
//        //        let feedbackState, innerState =
//        //            match s with
//        //            | None -> seed, None
//        //            | Some (my, inner) -> my, inner
//        //        match run (f feedbackState r) innerState r with
//        //        | Some res ->
//        //            let feed = fst res
//        //            let innerState = snd res
//        //            Some (fst feed, (snd feed, Some innerState))
//        //        | None -> None
//        //    |> create

//FeedbackBuilder(0) {
//    let! x = Gen.count0 ()
//    return 23
//}
//|> ignore
