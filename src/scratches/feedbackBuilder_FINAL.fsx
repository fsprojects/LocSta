
fsi.PrintWidth <- 100

#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
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
let forValue res = match res with | Value (v,s) -> v,s | _ -> failwith "unexpected"
let run g state = state |> Gen.asFunc g |> forValue


let count (start: string) (inc: int) =
    fdb {
        let! v = init start
        let nextValue = v + (string inc)
        return Res.feedback v nextValue
    }


//////////
// Tests
//////////

let g = count "start" 1

let v1,s1 = run g None
let v2,s2 = run g (Some s1)
let v3,s3 = run g (Some s2)

let res = count "start" 1 |> Gen.toSeqState |> Seq.truncate 2 |> Seq.toList
let res2 = count "start" 1 |> Gen.toListn 3


