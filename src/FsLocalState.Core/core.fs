
namespace FsLocalState

module Core =

    type Res<'a, 'b> =
        { value: 'a
          state: 'b }

    type Local<'value, 'state, 'reader> = Local of ('state option -> 'reader -> Res<'value, 'state>)

    let run m =
        let (Local b) = m in b

    // TODO: seems to be impossible having a single case DU here?
    type LocalInput<'inp, 'value, 'state, 'reader> = 'inp -> Local<'value, 'state, 'reader>

    type StateAcc<'a, 'b> =
        { mine: 'a
          exess: 'b }


    // -----
    // Monad
    // -----

    let bind (m: Local<'a, 'sa, 'r>) (f: 'a -> Local<'b, 'sb, 'r>): Local<'b, StateAcc<'sa, 'sb>, 'r> =
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

    let ret x =
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

    let inline binOpLeftRight left right f =
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

    let inline binOpLeft left right f =
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

    let inline binOpRight left right f =
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


    // -------
    // Kleisli
    // -------

    let kleisli (f: LocalInput<'a, 'b, _, _>) (g: LocalInput<'b, 'c, _, _>): LocalInput<'a, 'c, _, _> =
        fun x ->
            local {
                let! f' = f x
                return! g f' }
    let (>=>) = kleisli

    let kleisliPipe (f: Local<'a, _, _>) (g: LocalInput<'a, 'b, _, _>): Local<'b, _, _> =
        local {
            let! f' = f
            return! g f' }
    let (|=>) = kleisliPipe


    // -----------
    // map / apply
    // -----------

    let map local mapping =
        let f' s r =
            let res = (run local) s r
            let mappedRes = mapping res.value
            { value = mappedRes
              state = res.state }
        Local f'

    /// map operator
    let (<!>) = map

    let apply (f: Local<'v1 -> 'v2, _, 'r>) (l: Local<'v1, _, 'r>): Local<'v2, _, 'r> =
        local {
            let! l' = l
            let! f' = f
            let result = f' l'
            return result
        }

    /// apply operator
    let (<*>) = apply


    // ------
    // Reader
    // ------

    /// Reads the global state that is passed around to every loop function.
    let read () =
        Local(fun _ r ->
            { value = r
              state = () })


    // --------
    // Feedback
    // --------

    type Fbd<'a, 'b> =
        { feedback: 'a
          out: 'b }

    /// Feedback with reader state
    let (<|>) seed (f: 'a -> 'r -> Local<Fbd<'a, 'v>, 's, 'r>) =
        let f1 =
            fun prev r ->
                let myPrev, innerPrev =
                    match prev with
                    | None -> seed, None
                    | Some (my, inner) -> my, inner

                let lRes = run (f myPrev r) innerPrev r
                let feed = lRes.value
                let innerState = lRes.state
                { value = feed.out
                  state = feed.feedback, Some innerState }
        Local f1

    // /// Feedback without reader state
    // let (+->) seed f = (++>) seed (fun s _ -> f s)
