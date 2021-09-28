
[<AutoOpen>]
module FsLocalState.Core

type Res<'output, 'state> = 'output * 'state

type GenFunc<'output, 'state, 'reader> =
    'state option -> 'reader -> Res<'output, 'state> option

type StateFunc<'output, 'state, 'reader> =
    'state -> 'reader -> Res<'output, 'state> option

type Gen<'output, 'state, 'reader> =
    | Gen of GenFunc<'output, 'state, 'reader>

// TODO: seems to be impossible having a single case DU here?
type Eff<'input, 'output, 'state, 'reader> =
    'input -> Gen<'output, 'state, 'reader>

[<Struct>]
type StateAcc<'a, 'b> = { mine: 'a; exess: 'b }

module Gen =

    // --------
    // Construction / Init
    // --------

    let create (f: GenFunc<_,_,_>) = Gen f
    
    let initValue seed (f: StateFunc<_,_,_>) =
        fun s r ->
            let state = Option.defaultValue seed s
            f state r
        |> create

    let initWith seedFunc (f: StateFunc<_,_,_>) =
        fun s r ->
            let state =
                match s with
                | Some s -> s
                | None -> seedFunc r
            f state r
        |> create

    // -----
    // Monad
    // -----

    let run (Gen gen) = gen

    let bind
        (f: 'a -> Gen<'b, 'sb, 'r>)
        (m: Gen<'a, 'sa, 'r>) 
        : Gen<'b, StateAcc<'sa, 'sb>, 'r> 
        =
        fun s r ->
            let unpackedLocalState =
                match s with
                | None -> { mine = None; exess = None }
                | Some v -> { mine = Some v.mine; exess = Some v.exess }
            match (run m) unpackedLocalState.mine r with
            | Some m' ->
                let fGen = fst m' |> f
                match (run fGen) unpackedLocalState.exess r with
                | Some f' -> Some (fst f', { mine = snd m'; exess = snd f' })
                | None -> None
            | None -> None
        |> create

    let ret (x: 'a) =
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
    [<AbstractClass>]
    type GenBaseBuilder() =
        member _.Return x = ret x
        member _.ReturnFrom x = x
        member _.Zero () = zero ()

    type GenBuilder() =
        inherit GenBaseBuilder()
        member _.Bind(m, f) = bind f m
    
    //type FeedbackBuilder() =
    //    inherit GenBaseBuilder()
    //    member _.Bind(m, f) = bind f m

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
        fun s r -> s |> Option.map (fun s -> r,s)
        |> create

    /// Reads the local state.
    let state () =
        fun s _ -> s |> Option.map (fun s -> s,s)
        |> create

    /// Transforms a generator function to an effect function.    
    let toEff (gen: Gen<'s, 'r, 'o>) : Eff<unit, 's, 'r, 'o> =
        fun () -> gen

    let feedback seed (f: 'fdb -> 'r -> Gen<Res<'o, 'fdb>, 's, 'r>) =
        fun s r ->
            let feedbackState, innerState =
                match s with
                | None -> seed, None
                | Some (my, inner) -> my, inner
            match run (f feedbackState r) innerState r with
            | Some res ->
                let feedOutput,feedState = fst res
                let innerState = snd res
                Some (feedOutput, (feedState, Some innerState))
            | None -> None
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

    /// Bind
    let (>>=) m f = Gen.bind f m

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


//type Gen.GenBuilder with

//    [<CustomOperation("feedback")>]
//    member _.Feedback (m, f) = m
    
//    [<CustomOperation("init", MaintainsVariableSpaceUsingBind = true)>]
//    member _.Init (gen: Gen<_,_,_>, seed) =
//        fun s r -> Some (seed, seed)
//        |> Gen.create

//let init seed =
//    Gen.feedback seed (fun s r ->
//        gen {
//            return (1, 1)
//        }
//    )
//let set state =
//    fun s r ->
//        Some (state, state)
//    |> Gen.create
    

//let x seed = gen {
//    let! state = init seed
    
//    let result = state + 1

//    //feedback result

//    return result
//}

type FeedbackState<'feedbackState, 'innerState> = 'feedbackState * 'innerState option

type FeedbackBuilder<'a>(seed: 'a) =
    inherit Gen.GenBaseBuilder()
    member _.Bind (m: Gen<'o1, 's1, 'r>, f: 'o1 -> Gen<'o2, 's2_, 'r>) =
        // let feedback seed (f: 'fdb -> 'r -> Gen<Res<'o, 'fdb>, 's, 'r>) =
        fun feedbackState _ ->
            fun s r ->
                m >>= fun mRes ->
                    f mRes >>= fun fRes ->
                        Gen.ret (fRes, feedbackState)
            |> Gen.create
            //gen {
            //    let! mRes = m
            //    return! f (mRes, feedbackState)
            //}
        |> Gen.feedback seed
let feedback seed = FeedbackBuilder(seed)

/// Reads the feedback state.
let locals () =
    fun s _ ->
        match s with
        | Some (feedback,_ as s) -> Some (feedback, s)
        | None -> None
    |> Gen.create
