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
type State<'a, 'b> = { currState: 'a; subState: 'b }


module Gen =

    let internal run gen = let (Gen b) = gen in b

    // Creates a Gen from a function that takes optional state.
    let create f = Gen f
    
    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let createSeed seed f =
        fun s r ->
            let state = Option.defaultValue seed s
            f state r
        |> create

    let bind (f: 'o1 -> Gen<'o2, 's2, 'r>) (m: Gen<'o1, 's1, 'r>) : Gen<'o2, State<'s1, 's2>, 'r> =
        fun (s: State<'s1, 's2> option) r ->
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

    let feedback
        (seed: 'workingState)
        (f: 'workingState -> 'r -> Gen<'output * 'workingState, 'innerState, 'r>)
        : Gen<'output, 'workingState * 'innerState option, 'r>
        =
        fun (s: ('workingState * 'innerState option) option) (r: 'r) ->
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

    let ofValue x =
        fun _ _ -> Some (x, ())
        |> create

    let zero () =
        fun _ _ -> None
        |> create

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

    let apply (xGen: Gen<'o1, _, 'r>) (fGen: Gen<'o1 -> 'o2, _, 'r>) : Gen<'o2, _, 'r> =
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

    let kleisliFx (g: Eff<'b, 'c, _, _>) (f: Eff<'a, 'b, _, _>): Eff<'a, 'c, _, _> =
        fun x -> gen {
            let! f' = f x
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

    // TODO: Implement a random number generator that exposes it's serializable state.
    let private dotnetRandom = System.Random()
    let random<'a> () =
        fun _ (_: 'a) -> Some (dotnetRandom.NextDouble(), ())
        |> create

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

[<AutoOpen>]
module Operators =

    /// Feedback with reader state
    let (=>) seed f = Gen.feedback seed f

    /// Kleisli operator (eff >> eff)
    let (>=>) f g = Gen.kleisliFx g f

    /// Kleisli "pipe" operator (gen >> eff)
    let (|=>) f g = Gen.kleisli g f

let gen<'a> = Gen.gen<'a>
