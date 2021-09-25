[<AutoOpen>]
module FsLocalState.Core

type Res<'value, 'state> = 'value * 'state

type Gen<'value, 'state, 'reader> =
    | Gen of ('state option -> 'reader -> Res<'value, 'state> option)

// TODO: seems to be impossible having a single case DU here?
type Eff<'inp, 'value, 'state, 'reader> =
    'inp -> Gen<'value, 'state, 'reader>

[<Struct>]
type StateAcc<'a, 'b> = { mine: 'a; exess: 'b }

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

    let internal run gen =
        let (Gen b) = gen in b

    let internal bind
        (m: Gen<'a, 'sa, 'r>) 
        (f: 'a -> Gen<'b, 'sb, 'r>)
        : Gen<'b, StateAcc<'sa, 'sb>, 'r> 
        =
        let genFunc localState readerState =
            let unpackedLocalState =
                match localState with
                | None -> { mine = None; exess = None }
                | Some v -> { mine = Some v.mine; exess = Some v.exess }
            match (run m) unpackedLocalState.mine readerState with
            | Some m' ->
                let fGen = fst m' |> f
                match (run fGen) unpackedLocalState.exess readerState with
                | Some f' -> Some (fst f', { mine = snd m'; exess = snd f' })
                | None -> None
            | None -> None
        Gen genFunc

    let ret x =
        fun _ _ -> Some (x, ())
        |> Gen

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
    //    member _.Zero() = zero ()

    // TODO: other builder methods
    type GenBuilder() =
        member _.Bind(m, f) = bind m f
        member _.Return x = ret x
        member _.ReturnFrom x = x
        member _.Zero() = zero ()

    let gen = GenBuilder()
    

    // --------
    // map / apply
    // --------

    let map projection x =
        gen {
            match! x with
            | Some res -> return projection res
            | None -> ()
        }

    let apply (xGen: Gen<'v1, _, 'r>) (fGen: Gen<'v1 -> 'v2, _, 'r>): Gen<'v2, _, 'r> =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return result
        }


    // -------
    // Kleisli
    // -------

    let kleisli (g: Eff<'a, 'b, _, _>) (f: Gen<'a, _, _>): Gen<'b, _, _> =
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

    let feedback seed (f: 'a -> 'r -> Gen<Res<'v, 'a>, 's, 'r>) =
        fun s r ->
            let feedbackState, innerState =
                match s with
                | None -> seed, None
                | Some (my, inner) -> my, inner

            run (f feedbackState r) innerState r
            |> Option.map (fun res ->
                let feed = fst res
                let innerState = snd res
                fst feed, (snd feed, Some innerState)
            )
        |> create

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random() =
        fun _ _ -> Some (dotnetRandom.NextDouble(), ())
        |> create

    let countFrom inclusiveStart increment =
        fun s _ ->
            let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
            let newValue = state + increment
            Some (newValue, newValue)
        |> create

    let count0() = countFrom 0 1
    
    // TODO: countFloat

    let singletonValue value =
        fun s _ ->
            let instance = Option.defaultValue value s
            Some (instance, instance)
        |> create

    let singletonWith factory =
        fun s _ ->
            let instance = Option.defaultWith factory s
            Some (instance, instance)
        |> create

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
    static member inline (+)(left, right) = Gen.binOpBoth left right (+)
    static member inline (-)(left, right) = Gen.binOpBoth left right (-)
    static member inline (*)(left, right) = Gen.binOpBoth left right (*)
    static member inline (/)(left, right) = Gen.binOpBoth left right (/)
    static member inline (%)(left, right) = Gen.binOpBoth left right (%)
    
    static member inline (+)(left: float, right) = Gen.binOpLeft left right (+)
    static member inline (-)(left: float, right) = Gen.binOpLeft left right (-)
    static member inline (*)(left: float, right) = Gen.binOpLeft left right (*)
    static member inline (/)(left: float, right) = Gen.binOpLeft left right (/)
    static member inline (%)(left: float, right) = Gen.binOpLeft left right (%)

    static member inline (+)(left: int, right) = Gen.binOpLeft left right (+)
    static member inline (-)(left: int, right) = Gen.binOpLeft left right (-)
    static member inline (*)(left: int, right) = Gen.binOpLeft left right (*)
    static member inline (/)(left: int, right) = Gen.binOpLeft left right (/)
    static member inline (%)(left: int, right) = Gen.binOpLeft left right (%)

    static member inline (+)(left, right: float) = Gen.binOpRight left right (+)
    static member inline (-)(left, right: float) = Gen.binOpRight left right (-)
    static member inline (*)(left, right: float) = Gen.binOpRight left right (*)
    static member inline (/)(left, right: float) = Gen.binOpRight left right (/)
    static member inline (%)(left, right: float) = Gen.binOpRight left right (%)

    static member inline (+)(left, right: int) = Gen.binOpRight left right (+)
    static member inline (-)(left, right: int) = Gen.binOpRight left right (-)
    static member inline (*)(left, right: int) = Gen.binOpRight left right (*)
    static member inline (/)(left, right: int) = Gen.binOpRight left right (/)
    static member inline (%)(left, right: int) = Gen.binOpRight left right (%)


[<AutoOpen>]
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

let gen = Gen.gen
