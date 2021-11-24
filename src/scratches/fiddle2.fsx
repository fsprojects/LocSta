
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
open FsLocalState.Lib.Gen

loop {
    return! count 0 1
    for x in [0..10] do
        let! y = loop { 5 }
        yield x + y
}
|> Gen.toListn 10


//gen {
//    let! c = count 0 1
//    return Control.Emit c
//}
//|> Gen.toListn 10

//fdb {
//    let! state = Init "Hello"
//    let! c1 = count 0 1
//    let! c2 = count 5 1
//    return Control.Feedback ($"{state} {c1}-{c2}", "")
//}
//|> Gen.toListn 10


let bind
    (f: 'o1 -> GenForGen<'o2, 's2>)
    (m: GenForGen<'o1, 's1>)
    : GenForGen<'o2, GenState<'s1, 's2, GenResultGen<'o1, 's1>>>
    =
    fun state ->
        failwith ""
    |> Gen.createGen

let bindInitFdbGen
    (f: 'f -> GenForFdb<'o,'f,'s>)
    (m: Init<'f>)
    : GenForGen<_,_>
    =
    fun state ->
        failwith ""
    |> Gen.createGen

let bindGenFdbFdb
    (f: 'o1 -> GenForFdb<'o2, 'f, 's2>)
    (m: GenForGen<'o1, 's1>)
    : GenForFdb<'o1, 'f, 's2> // TODO
    =
    fun state ->
        failwith ""
    |> Gen.createFdb

let a () =
    Init 0 |> bindInitFdbGen (fun state ->
    count 0 1 |> bindGenFdbFdb (fun c1 ->
    count 0 1 |> bindGenFdbFdb (fun c2 ->
    Gen.returnFeedback state (c1 + c2)
    )))


//let x =
//    Init 0 |> Gen.bindInitFdb (fun state ->
//        Gen.returnFeedback state 12
//    )

//let y =
//    Init 0 |> Gen.bindInitFdb (fun state ->
//        count 0 1 |> Gen.bindGenFdb (fun c ->
//            Gen.returnFeedback state (c + 1)
//        )
//    )


let x = ResizeArray<int>()

x.Add 1
x.Add 2
x.Add 3

x

type Loop = Loop of int
type Feed = Feed of string
type Res<'a> = Res of 'a

let (|MatchLoop|) x = match x with Loop i -> i
let (|MatchFeed|) x = match x with Feed s -> s

let create res (|State|) =
    match res with
    | Res ((State s) as inp) ->
        s, Res inp

create (Res(Loop 5)) (|MatchLoop|)
create (Res(Feed "5")) (|MatchFeed|)
