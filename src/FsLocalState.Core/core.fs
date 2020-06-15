namespace FsLocalState

[<AutoOpen>]
module Core =

    type Res<'a, 'b> =
        { value: 'a
          state: 'b }

    type Gen<'value, 'state, 'reader> = Gen of ('state option -> 'reader -> Res<'value, 'state>)

    // TODO: seems to be impossible having a single case DU here?
    type Eff<'inp, 'value, 'state, 'reader> = 'inp -> Gen<'value, 'state, 'reader>

    type StateAcc<'a, 'b> =
        { mine: 'a
          exess: 'b }


    // -----
    // Monad
    // -----

    let internal run gen = let (Gen b) = gen in b

    let internal bind (m: Gen<'a, 'sa, 'r>) (f: 'a -> Gen<'b, 'sb, 'r>): Gen<'b, StateAcc<'sa, 'sb>, 'r> =
        let genFunc localState readerState =
            let unpackedLocalState =
                match localState with
                | None ->
                    { mine = None
                      exess = None }
                | Some v ->
                    { mine = Some v.mine
                      exess = Some v.exess }

            let m' = (run m) unpackedLocalState.mine readerState
            let fGen = f m'.value
            let f' = (run fGen) unpackedLocalState.exess readerState

            { value = f'.value
              state =
                  { mine = m'.state
                    exess = f'.state } }

        Gen genFunc

    let internal ret x =
        fun _ _ ->
            { value = x
              state = () }
        |> Gen


    // -------
    // Builder
    // -------

    // TODO: Docu
    // TODO: other builder methods
    type GenBuilderEx<'a>() =
        member __.Bind(m: Gen<_, _, 'a>, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom x = x

    // TODO: other builder methods
    type GenBuilder() =
        member __.Bind(m, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom x = x

    let gen = GenBuilder()


    // ----------
    // Arithmetik
    // ----------

    let inline internal binOpLeftRight left right f =
        gen {
            let! l = left
            let! r = right
            return f l r }

    type Gen<'v, 's, 'r> with
        static member inline (+)(left, right) = binOpLeftRight left right (+)
        static member inline (-)(left, right) = binOpLeftRight left right (-)
        static member inline (*)(left, right) = binOpLeftRight left right (*)
        static member inline (/)(left, right) = binOpLeftRight left right (/)
        static member inline (%)(left, right) = binOpLeftRight left right (%)

    let inline internal binOpLeft left right f =
        gen {
            let l = left
            let! r = right
            return f l r
        }

    type Gen<'v, 's, 'r> with
        static member inline (+)(left, right) = binOpLeft left right (+)
        static member inline (-)(left, right) = binOpLeft left right (-)
        static member inline (*)(left, right) = binOpLeft left right (*)
        static member inline (/)(left, right) = binOpLeft left right (/)
        static member inline (%)(left, right) = binOpLeft left right (%)

    let inline internal binOpRight left right f =
        gen {
            let! l = left
            let r = right
            return f l r
        }

    type Gen<'v, 's, 'r> with
        static member inline (+)(left, right) = binOpRight left right (+)
        static member inline (-)(left, right) = binOpRight left right (-)
        static member inline (*)(left, right) = binOpRight left right (*)
        static member inline (/)(left, right) = binOpRight left right (/)



module Res =

    let getValue (x: Res<_, _>) = x.value


module Eff =

    // -------
    // Kleisli
    // -------

    let kleisli (g: Eff<'b, 'c, _, _>) (f: Eff<'a, 'b, _, _>): Eff<'a, 'c, _, _> =
        fun x ->
            gen {
                let! f' = f x
                return! g f' }

    
module Gen =

    let run gen = Core.run gen
    
    let bind f gen = Core.bind gen f

    /// Return function.
    let ret gen = Core.ret gen

    /// Lifts a generator function to an effect function.    
    let toEff (gen: Gen<'s, 'r, 'o>): Eff<unit, 's, 'r, 'o> =
        fun () -> gen

    
    // ----------
    // Arithmetik
    // ----------

    let inline binOpLeftRight left right f = Core.binOpLeftRight left right f
    
    let inline binOpLeft left right f = Core.binOpLeft left right f
    
    let inline binOpRight left right f = Core.binOpRight left right f
    

    // -------
    // Kleisli
    // -------

    let kleisli (g: Eff<'a, 'b, _, _>) (f: Gen<'a, _, _>): Eff<unit,'b, _, _> =
        Eff.kleisli g (toEff f)

   
    // -----------
    // map / apply
    // -----------

    let map projection gen =
        fun s r ->
            let res = (run gen) s r
            let mappedRes = projection res.value
            { value = mappedRes
              state = res.state }
        |> Gen

    let apply (xGen: Gen<'v1, _, 'r>) (fGen: Gen<'v1 -> 'v2, _, 'r>): Gen<'v2, _, 'r> =
        gen {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            return result
        }


    // ------
    // Reader
    // ------

    /// Reads the global state.
    let read () =
        fun _ r ->
            { value = r
              state = () }
        |> Gen


    // --------
    // Feedback / Init
    // --------

    let init seed f =
        fun s r ->
            let state = Option.defaultValue seed s
            f state r
        |> Gen

    let init2 f seed = init f seed

    let feedback (f: 'a -> 'r -> Gen<Res<'v, 'a>, 's, 'r>) seed =
        fun s r ->
            let feedbackState, innerState =
                match s with
                | None -> seed, None
                | Some (my, inner) -> my, inner

            let res = run (f feedbackState r) innerState r
            let feed = res.value
            let innerState = res.state
            { value = feed.value
              state = feed.state, Some innerState }
        |> Gen



[<AutoOpen>]
module Operators =

    /// Feedback with reader state
    let (<|>) seed f = Gen.feedback f seed

    /// map operator
    let (<!>) gen projection = Gen.map projection gen

    /// apply operator
    let (<*>) fGen xGen = Gen.apply xGen fGen

    /// Kleisli operator (eff >> eff)
    let (>=>) f g = Eff.kleisli g f

    /// Kleisli "pipe" operator (gen >> eff)
    let (|=>) f g = Gen.kleisli g f
