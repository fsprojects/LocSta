
fsi.PrintWidth <- 100

#r "../LocSta/bin/Debug/netstandard2.0/LocSta.dll"
open LocSta

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

open LocSta
open LocSta.Gen



type F1<'f, 'o, 's> = 'f -> 's option -> GenResult<('o * 'f), 's>

let f1ToGen m =
    fun feedback -> ((fun state -> m feedback state) |> Gen.create)

let bindFeedback4 m f =
    fun feedback state ->
        let res = Gen.feedback feedback (f1ToGen m) |> Gen.bind f
        (Gen.run res) state

type FeedbackBuilder<'a>(seed : 'a) =
    member this.Bind(m, f) =
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
                let fgen = f mres last_feed
                match (Gen.run fgen) last_fstate with
                | Value ((fres, ffeed), fstate) ->
                    Value (fres, { mine = ffeed; inner = Some { currState = mstate; subState = Some fstate } })
                    //Value (fr, { mine = ff; inner = Some fs })
                | _ -> failwith "TODO"
            | _ -> failwith "TODO"
        |> Gen.create


            //match run (f feedbackState) innerState with
            //| Value ((resF, feedStateF), innerStateF) ->
            //    Value (resF, { mine = feedStateF; inner = Some innerStateF })
            //| Discard (Some innerStateF) -> 
            //    Discard (Some { mine = seed; inner = Some innerStateF })
            //| Discard None -> Discard None
            //| Stop -> Stop
        //|> Gen.create

        //seed => fun state -> gen {
        //    let! mres = m state
        //    let fm = f mres
        //    let! fres = seed => fun state -> fm state
        //    return fres
        //}

        //seed => (fun feedback -> gen {
        //    printfn "FFFFFFFF %A" feedback
        //    let! mres = m feedback
        //    return! f mres
        //})
        
        //fun state ->
        //    let a = "sdfsdf"
        //    let res = Gen.feedback seed (f1ToGen m) |> Gen.bind f
        //    let res2 = (runGen res) state
        //    res2
        //|> Gen.create

        //(bindFeedback4 m f |> f1ToGen) seed
    member this.Return(x) = fun _ -> Gen.ofResult x

let inline fdb seed = FeedbackBuilder(seed)

let state =
    fun feedback -> gen {
        return Res.feedback feedback feedback
    }

let count (start: string) (inc: int) =
    fdb start {
        let! v = state
        let nextValue = v + (string inc)
        return Value ((v, nextValue), ())
    }


let g = count "start" 1

let v1,s1 = run g None
let v2,s2 = run g (Some s1)
let v3,s3 = run g (Some s2)

let res = count "start" 1 |> Gen.toSeqState |> Seq.truncate 2 |> Seq.toList
let res2 = count "start" 1 |> Gen.toListn 10

fdb () {
    let! s = state
    let! v1 = count "Hallo" 1
    return Value ((v1 + v2, ()), ())
}


//let count start inc =
//    fdb {
//        let! v = init ()
//        let nextValue = v + inc
//        return Value ((v, nextValue), ())
//    }

//let res = count 0 1 |> f1ToGen
//0 |> res |> Gen.toListn 10
