
#r "../../src/LocSta/bin/Debug/netstandard2.0/LocSta.dll"

open System
open LocSta


let monteCarlo() =
    feed {
        let! lastInsideCount = Init 0
        let! samples = Gen.count 1 1
        let! x = Gen.random()
        let! y = Gen.random()
        let distance = Math.Sqrt (x*x + y*y)
        let isInsideCircle = distance < 1.0
        let insideCount = if isInsideCircle then lastInsideCount + 1 else lastInsideCount
        let pi = 4.0 * float insideCount / float samples
        yield pi, insideCount
    }


// evaluate pi
let pi = monteCarlo() |> Gen.toSeq |> Seq.take 1_000_000 |> Seq.last

printfn "pi ~= %f" pi


//// you can store state somewhere...
//// load it and resume (we take only 1 additional sample and still get close to pi):
//let resumedSeq = monteCarlo |> Gen.resume state
//let piResumed, stateResumed = piSeq |> Seq.take 10_000 |> Seq.last

//printfn "pi_more_accurate ~= %f" piResumed

//// ...or list all n incremental calculated values
//let values = monteCarlo |> Gen.toList 1000
