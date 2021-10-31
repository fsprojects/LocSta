
fsi.PrintWidth <- 100

#r "FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
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

/////////
// Helper
/////////
module Gen =
    let run g = match g with | Gen g -> g
let forValue res = match res with | Value (v,s) -> v,s | _ -> failwith "unexpected"
let run g state = state |> Gen.run g |> forValue

open FsLocalState.Core
open FsLocalState.Core.Gen


type FeedbackBuilder() =
    member this.Bind(m: 'f -> Gen<('a * 'f), 'sm>, f) =
        fun seed ->
            fun state ->
                let last_feed, last_mstate, last_fstate =
                    match state with
                    | None -> seed, None, None
                    | Some { mine = mine; inner = inner } ->
                        match inner with
                        | None -> mine, None, None
                        | Some v -> mine, Some v.currState, v.subState
                let mgen = m last_feed
                match (Gen.run mgen) last_mstate with
                | Value ((mres, mfeed), mstate) ->
                    // TODO: mf is discarded - that sound ok
                    let fgen = f mres
                    match (Gen.run fgen) last_fstate with
                    | Value ((fres, ffeed), fstate) ->
                        Value (
                            fres, 
                            { mine = ffeed
                              inner = Some { currState = mstate
                                             subState = Some fstate } }
                        )
                    | _ -> failwith "TODO"
                | _ -> failwith "TODO"
            |> Gen.create
    member this.Return(x) = Gen.ofResult x

let fdb = FeedbackBuilder()

let state =
    fun feedback -> gen {
        return Res.feedback feedback feedback
    }

let count (start: string) (inc: int) =
    start |> fdb {
        let! v = state
        let nextValue = v + (string inc)
        return Value ((v, nextValue), ())
    }


//////////
// Tests
//////////

let g = count "start" 1

let v1,s1 = run g None
let v2,s2 = run g (Some s1)
let v3,s3 = run g (Some s2)

let res = count "start" 1 |> Gen.toSeqState |> Seq.truncate 2 |> Seq.toList
let res2 = count "start" 1 |> Gen.toListn 10



//() |> fdb {
//    let! s = state
//    let! v = count "Hallo" 1
//    return Value ((0, ()), ())
//}
