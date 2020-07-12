
[<AutoOpen>]
module FsLocalState.Core

[<Struct>]
type Res<'a, 'b> =
    { value: 'a
      state: 'b }

type Gen<'value, 'state, 'reader> =
    Gen of ('state option -> 'reader -> Res<'value, 'state>)

// TODO: seems to be impossible having a single case DU here?
type Eff<'inp, 'value, 'state, 'reader> = 'inp -> Gen<'value, 'state, 'reader>

[<Struct>]
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

let inline internal binOpBoth left right f =
    gen {
        let! l = left
        let! r = right
        return f l r }

type Gen<'v, 's, 'r> with
    static member inline (+)(left, right) = binOpBoth left right (+)
    static member inline (-)(left, right) = binOpBoth left right (-)
    static member inline (*)(left, right) = binOpBoth left right (*)
    static member inline (/)(left, right) = binOpBoth left right (/)
    static member inline (%)(left, right) = binOpBoth left right (%)

let inline internal binOpLeft left right f =
    gen {
        let l = left
        let! r = right
        return f l r
    }
    
// TODO: Generic overload resolution isn't working (compiler says it's too complex) having binOpLeft, binOpRight and binOpBoth.
//       So add more relevant types here.

type Gen<'v, 's, 'r> with
    static member inline (+)(left: float, right) = binOpLeft left right (+)
    static member inline (-)(left: float, right) = binOpLeft left right (-)
    static member inline (*)(left: float, right) = binOpLeft left right (*)
    static member inline (/)(left: float, right) = binOpLeft left right (/)
    static member inline (%)(left: float, right) = binOpLeft left right (%)

    static member inline (+)(left: int, right) = binOpLeft left right (+)
    static member inline (-)(left: int, right) = binOpLeft left right (-)
    static member inline (*)(left: int, right) = binOpLeft left right (*)
    static member inline (/)(left: int, right) = binOpLeft left right (/)
    static member inline (%)(left: int, right) = binOpLeft left right (%)

let inline internal binOpRight left right f =
    gen {
        let! l = left
        let r = right
        return f l r
    }

type Gen<'v, 's, 'r> with
    static member inline (+)(left, right: float) = binOpRight left right (+)
    static member inline (-)(left, right: float) = binOpRight left right (-)
    static member inline (*)(left, right: float) = binOpRight left right (*)
    static member inline (/)(left, right: float) = binOpRight left right (/)
    static member inline (%)(left, right: float) = binOpRight left right (%)

    static member inline (+)(left, right: int) = binOpRight left right (+)
    static member inline (-)(left, right: int) = binOpRight left right (-)
    static member inline (*)(left, right: int) = binOpRight left right (*)
    static member inline (/)(left, right: int) = binOpRight left right (/)
    static member inline (%)(left, right: int) = binOpRight left right (%)

