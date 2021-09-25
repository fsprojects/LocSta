
[<AutoOpen>]
module FsLocalState.Core

type Res<'value, 'state> = 'value * 'state

type Gen<'value, 'state, 'reader> =
    private
    | Gen of ('state option -> 'reader -> Res<'value, 'state> option)

// TODO: seems to be impossible having a single case DU here?
type Eff<'inp, 'value, 'state, 'reader> =
    'inp -> Gen<'value, 'state, 'reader>

[<Struct>]
type StateAcc<'a, 'b> = { mine: 'a; exess: 'b }

module Gen =
    let createForOption f = Gen f
    let createForValue f = Gen (fun s r -> Some (f s r))

// -----
// Monad
// -----

let internal run gen = let (Gen b) = gen in b

let internal bind
    (m: Gen<'a, 'sa, 'r>) 
    (f: 'a -> Gen<'b, 'sb, 'r>)
    : Gen<'b, StateAcc<'sa, 'sb>, 'r> 
    =
    let genFunc localState readerState =
        let unpackedLocalState =
            match localState with
            | None ->
                { mine = None
                  exess = None }
            | Some v ->
                { mine = Some v.mine
                  exess = Some v.exess }

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

// -------
// Builder
// -------

// TODO: Docu
// TODO: other builder methods
type GenBuilderEx<'a>() =
    member _.Bind(m: Gen<_, _, 'a>, f) = bind m f
    member _.Return x = ret x 
    member _.ReturnFrom x = x
    member _.Zero() = zero ()

// TODO: other builder methods
type GenBuilder() =
    member _.Bind(m, f) = bind m f
    member _.Return x = ret x
    member _.ReturnFrom x = x
    member _.Zero() = zero ()

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

