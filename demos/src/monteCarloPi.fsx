
#r "../lib/FsLocalState.dll"

open System
open FsLocalState
open FsLocalState.Operators


let monteCarlo =
    0 <|> fun lastInsideCount (_: unit) -> gen {
        let! samples = Gen.countFrom 1 1
        let! x = Gen.random()
        let! y = Gen.random()
        let distance = Math.Sqrt (x*x + y*y)
        let isInsideCircle = distance < 1.0
        // let! insideCount = isInsideCircle <?> count <!> lastInsideCount
        let insideCount = if isInsideCircle then lastInsideCount + 1 else lastInsideCount
        let pi = 4.0 * float insideCount / float samples
        return { value = pi; state = insideCount }
    }



#time

// evaluate pi
let piSeq = monteCarlo |> Gen.toSeqWithState ignore
let { value = pi; state = state } = piSeq |> Seq.take 1_000_000 |> Seq.last

printfn "pi ~= %f" pi

// you can store state somewhere...
// load it and resume (we take only 1 additional sample and still get close to pi):
let resumedSeq = Gen.resume ignore state monteCarlo
let { value = piResumed; state = stateResumed } = piSeq |> Seq.take 10_000 |> Seq.last

printfn "pi_more_accurate ~= %f" piResumed

// ...or list all n incremental calculated values
let values = monteCarlo |> Gen.toSeq ignore |> Seq.take 1000 |> Seq.toList
