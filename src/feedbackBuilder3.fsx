
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
    member this.Bind(m: Gen<'o1, 's1>, f: 'o1 -> Gen<'o2, 's2>) : Gen<'o2, State<'s1, 's2>> =
        fun (state: State<'s1, 's2> option) ->
            let lastMState, lastFState =
                match state with
                | None -> None, None
                | Some v -> Some v.currState, v.subState
            match (Gen.run m) lastMState with
            | Value (mres, mstate) ->
                let fGen = f mres
                match (Gen.run fGen) lastFState with
                | Value (fres, fstate) -> Value (fres, { currState = mstate; subState = Some fstate })
                | Discard stateF -> Discard (Some { currState = mstate; subState = stateF })
                | Stop -> Stop
            | Discard (Some stateM) -> Discard (Some { currState = stateM; subState = lastFState })
            | Discard None ->
                match lastMState with
                | Some lastStateM -> Discard (Some { currState = lastStateM; subState = lastFState })
                | None -> Discard None
            | Stop -> Stop
        |> create
    member this.Bind(m: 'f option -> Gen<('o1 * 'f), 's1>, f: 'o1 -> Gen<('o2 * 'f option), 's2>) =
        fun state ->
            let lastFeed, lastMSstate, lastFState =
                match state with
                | None -> None, None, None
                | Some { mine = mine; inner = inner } ->
                    match inner with
                    | None -> mine, None, None
                    | Some v -> mine, Some v.currState, v.subState
            let mgen = m lastFeed
            match (Gen.run mgen) lastMSstate with
            | Value ((mres, mfeed), mstate) ->
                // TODO: mf is discarded - that sound ok
                let fgen = f mres
                match (Gen.run fgen) lastFState with
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

let init seed =
    fun feedback -> gen {
        let feedback = feedback |> Option.defaultValue seed
        return Res.feedback feedback feedback
    }

let count (start: string) (inc: int) =
    fdb {
        let! v = init start
        let nextValue = v + (string inc)
        return Value ((v, Some nextValue), ())
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



fdb {
    let! s = init 'a'
    let nextChar = 
        System.Convert.ToByte s
        |> fun x -> x + 1uy
        |> System.Convert.ToChar
    let! v = count "Hallo" 1
    let res = (string s) + "-" + v
    return Value ((res, Some nextChar), ())
}
|> Gen.toListn 3
