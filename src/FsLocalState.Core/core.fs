namespace FsLocalState

[<AutoOpen>]
module Core =

    type Res<'a, 'b> =
        { value: 'a
          state: 'b }

    type Local<'value, 'state, 'reader> = Local of ('state option -> 'reader -> Res<'value, 'state>)

    // TODO: seems to be impossible having a single case DU here?
    type LocalInput<'inp, 'value, 'state, 'reader> = 'inp -> Local<'value, 'state, 'reader>

    type StateAcc<'a, 'b> =
        { mine: 'a
          exess: 'b }


    // -----
    // Monad
    // -----

    let internal run local = let (Local b) = local in b

    let internal bind (m: Local<'a, 'sa, 'r>) (f: 'a -> Local<'b, 'sb, 'r>): Local<'b, StateAcc<'sa, 'sb>, 'r> =
        let localFunc localState readerState =
            let unpackedLocalState =
                match localState with
                | None ->
                    { mine = None
                      exess = None }
                | Some v ->
                    { mine = Some v.mine
                      exess = Some v.exess }

            let m' = (run m) unpackedLocalState.mine readerState
            let fLocal = f m'.value
            let f' = (run fLocal) unpackedLocalState.exess readerState

            { value = f'.value
              state =
                  { mine = m'.state
                    exess = f'.state } }

        Local localFunc

    let internal ret x =
        Local(fun _ _ ->
            { value = x
              state = () })


    // -------
    // Builder
    // -------

    // TODO: Docu
    // TODO: other builder methods
    type LocalReaderBuilder<'a>() =
        member __.Bind(m: Local<_, _, 'a>, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom x = x

    // TODO: other builder methods
    type LocalBuilder() =
        member __.Bind(m, f) = bind m f
        member __.Return x = ret x
        member __.ReturnFrom x = x

    let local = LocalBuilder()


    // ----------
    // Arithmetik
    // ----------

    let inline internal binOpLeftRight left right f =
        local {
            let! l = left
            let! r = right
            return f l r }

    type Local<'v, 's, 'r> with
        static member inline (+)(left, right) = binOpLeftRight left right (+)
        static member inline (-)(left, right) = binOpLeftRight left right (-)
        static member inline (*)(left, right) = binOpLeftRight left right (*)
        static member inline (/)(left, right) = binOpLeftRight left right (/)
        static member inline (%)(left, right) = binOpLeftRight left right (%)

    let inline internal binOpLeft left right f =
        local {
            let l = left
            let! r = right
            return f l r
        }

    type Local<'v, 's, 'r> with
        static member inline (+)(left, right) = binOpLeft left right (+)
        static member inline (-)(left, right) = binOpLeft left right (-)
        static member inline (*)(left, right) = binOpLeft left right (*)
        static member inline (/)(left, right) = binOpLeft left right (/)
        static member inline (%)(left, right) = binOpLeft left right (%)

    let inline internal binOpRight left right f =
        local {
            let! l = left
            let r = right
            return f l r
        }

    type Local<'v, 's, 'r> with
        static member inline (+)(left, right) = binOpRight left right (+)
        static member inline (-)(left, right) = binOpRight left right (-)
        static member inline (*)(left, right) = binOpRight left right (*)
        static member inline (/)(left, right) = binOpRight left right (/)



module Local =
    
    let value (x: Res<_, _>) = x.value
    
    let run local = Core.run local
    
    let bind f local = Core.bind local f
    
    let ret local = Core.ret local

    /// Lifts a generator to an effect    
    let lift (local: Local<'s, 'r, 'o>): LocalInput<unit, 's, 'r, 'o> =
        fun () -> local

    
    // ----------
    // Arithmetik
    // ----------

    let inline binOpLeftRight left right f = Core.binOpLeftRight left right f
    
    let inline binOpLeft left right f = Core.binOpLeft left right f
    
    let inline binOpRight left right f = Core.binOpRight left right f
    

    // -------
    // Kleisli
    // -------

    let kleisli (g: LocalInput<'b, 'c, _, _>) (f: LocalInput<'a, 'b, _, _>): LocalInput<'a, 'c, _, _> =
        fun x ->
            local {
                let! f' = f x
                return! g f' }

    let kleisliGen (g: LocalInput<'a, 'b, _, _>) (f: Local<'a, _, _>): Local<'b, _, _> =
        local {
            let! f' = f
            return! g f' }


    // -----------
    // map / apply
    // -----------

    let map projection local =
        fun s r ->
            let res = (run local) s r
            let mappedRes = projection res.value
            { value = mappedRes
              state = res.state }
        |> Local

    let apply (l: Local<'v1, _, 'r>) (f: Local<'v1 -> 'v2, _, 'r>): Local<'v2, _, 'r> =
        local {
            let! l' = l
            let! f' = f
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
        |> Local


    // --------
    // Feedback / Init
    // --------

    let init seed f =
        fun s r ->
            let state = Option.defaultValue seed s
            f state r
        |> Local

    let init2 f seed = init f seed

    let feedback (f: 'a -> 'r -> Local<Res<'v, 'a>, 's, 'r>) seed =
        fun s r ->
            let feedbackState, innerState =
                match s with
                | None -> seed, None
                | Some (my, inner) -> my, inner

            let lRes = run (f feedbackState r) innerState r
            let feed = lRes.value
            let innerState = lRes.state
            { value = feed.value
              state = feed.state, Some innerState }
        |> Local
        
[<AutoOpen>]
module Operators =

    /// Feedback with reader state
    let (<|>) seed f = Local.feedback f seed

    /// map operator
    let (<!>) local projection = Local.map projection local

    /// apply operator
    let (<*>) f l = Local.apply l f

    let (>=>) f g = Local.kleisli g f

    let (|=>) f g = Local.kleisliGen g f
