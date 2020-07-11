
#r "./lib/FsLocalState.dll"

open System
open FsLocalState


let countFrom seed increment =
    (seed - 1) <|> fun state (_: unit) -> gen {
        let newValue = state + increment
        return { value = newValue; state = newValue }
    }

let test =
    gen {
        let! a = countFrom 1 1
        let! b = countFrom 1 1
        let! c = countFrom 1 1
        return a + b + c
    }

#time

// evaluate pi
let value = test |> Eval.Gen.toSeq ignore |> Seq.take 5_000_000 |> Seq.last
