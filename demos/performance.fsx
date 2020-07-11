
#r "./lib/FsLocalState.dll"

open System
open FsLocalState


let countFrom seed increment =
    (seed - 1) <|> fun state (_: unit) -> gen {
        let newValue = state + increment
        return { value = newValue; state = newValue }
    }

let dummy1 =
    fun s (_: unit) ->
        let s = Option.defaultValue 0 s
        let v = s + 1
        { value = v; state = v }
    |> Gen

let dummy2 =
    fun s (_: unit) ->
        let v = s + 1
        { value = v; state = v }
    |> Gen.init 0

let dummy3 =
    0 <|> fun state (_: unit) -> gen {
        let v = state + 1
        return { value = v; state = v }
    }

let test1 =
    gen {
        let! a = dummy1
        let! b = dummy1
        let! c = dummy1
        return ()
    }

let test2 =
    gen {
        let! a = dummy2
        let! b = dummy2
        let! c = dummy2
        return ()
    }

let test3 =
    gen {
        let! a = dummy3
        let! b = dummy3
        let! c = dummy3
        return ()
    }

#time

// evaluate pi
let res1 = test1 |> Eval.Gen.toSeq ignore |> Seq.take 5_000_000 |> Seq.last
let res2 = test2 |> Eval.Gen.toSeq ignore |> Seq.take 5_000_000 |> Seq.last
let res3 = test3 |> Eval.Gen.toSeq ignore |> Seq.take 5_000_000 |> Seq.last
