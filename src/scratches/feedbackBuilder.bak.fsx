
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

(*
let bindTest f =
    ((fun state -> Stop) |> Gen.create) |> Gen.bind f

let bindTest1 f =
    let m = (fun state -> Stop) |> Gen.create
    m |> Gen.bind f

let bindTest2 m f =
    m |> Gen.bind f

let bindFeedbackTest f =
    fun seed -> Gen.feedback seed (fun state -> gen { return Res.stop }) |> Gen.bind f

let bindFeedback f =
    let m = fun state -> gen { return Res.stop }
    fun seed -> Gen.feedback seed m |> Gen.bind f

let bindFeedback1 f =
    let m = fun myState -> (fun state -> Stop) |> Gen.create
    fun seed -> Gen.feedback seed m |> Gen.bind f

let bindFeedback2 m f =
    fun seed -> Gen.feedback seed m |> Gen.bind f

type F<'a, 'b, 'c> = F of ('a -> Gen<('b * 'a), 'c>)

let feedback m seed =
    match m with | F m -> Gen.feedback seed m

let bindFeedback3 m f =
    let m = (fun myState -> ((fun state -> Stop) |> Gen.create)) |> F
    (fun seed -> feedback m seed |> Gen.bind f) |> F
*)


type F1<'f, 'o, 's> =
    | F1 of ('f -> 's option -> GenResult<('o * 'f), 's>)

let f1ToGen f1 =
    match f1 with
    | F1 m -> (fun myState -> ((fun state -> m myState state) |> Gen.create))

let bindFeedback4 m f =
    fun feedback state ->
        let res = (fun seed -> Gen.feedback seed (f1ToGen m) |> Gen.bind f)
        match res feedback with
        | Gen g -> g state
    |> F1

type FeedbackBuilder(seed) =
    member this.Bind(m, f) = (bindFeedback4 m f |> f1ToGen) seed
    member this.Return(x) = Gen.ofResult x

let fdb seed = FeedbackBuilder(seed)

let inline state () =
    fun myState state ->
        printfn "myState: %A   -   state: %A" myState state
        Value ((myState, myState), ())
    |> F1

let count start inc =
    fdb start {
        let! v = state ()
        let nextValue = v + inc
        return Value ((v, nextValue), ())
    }

let res = count 0 1 |> Gen.toListn 10


//let count start inc =
//    fdb {
//        let! v = init ()
//        let nextValue = v + inc
//        return Value ((v, nextValue), ())
//    }

//let res = count 0 1 |> f1ToGen
//0 |> res |> Gen.toListn 10
