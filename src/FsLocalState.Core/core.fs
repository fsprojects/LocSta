
[<AutoOpen>]
module FsLocalState.Core

type Res<'value, 'state> = ('value * 'state)
type GenOptionType<'value, 'state, 'reader> = 'state option -> 'reader -> Res<'value, 'state> option
type GenOptionNoReaderType<'value, 'state> = 'state option -> Res<'value, 'state> option
type GenType<'value, 'state, 'reader> = 'state option -> 'reader -> Res<'value, 'state>
type GenNoReaderType<'value, 'state> = 'state option -> Res<'value, 'state>

type Gen<'value, 'state, 'reader> =
    private
    | Gen of GenOptionType<'value, 'state, 'reader> with
    static member create(f: GenOptionType<'value, 'state, 'reader>) =
        Gen f
    static member create(f: GenType<'value, 'state, 'reader>) =
        Gen(fun s r -> Some (f s r))
    static member create(f: GenOptionNoReaderType<'value, 'state>) =
        Gen(fun s _ -> f s)
    static member create(f: GenNoReaderType<'value, 'state>) =
        Gen(fun s _ -> Some(f s))

// TODO: seems to be impossible having a single case DU here?
type Eff<'inp, 'value, 'state, 'reader> =
    'inp -> Gen<'value, 'state, 'reader>

[<Struct>]
type StateAcc<'a, 'b> = { mine: 'a; exess: 'b }


// -----
// Monad
// -----

let internal run gen = let (Gen b) = gen in b

let internal bind
    (m: Gen<'a, 'sa, 'r>) 
    (cont: 'a -> Gen<'b, 'sb, 'r>)
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
        | Some (mValue,mState) ->
            let contGen = cont mValue
            match (run contGen) unpackedLocalState.exess readerState with
            | Some (contValue,contState) ->
                Some(contValue, { mine = mState; exess = contState })
            | None -> None
        | None -> None

    Gen genFunc

let internal ret x = (fun _ _ -> Some (x, ())) |> Gen


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

