
#r "./lib/FsLocalState.dll"

open System
open FsLocalState

// TODO: Move these helper methods in FsLocalState
[<AutoOpen>]
module Helper =

    // Ok, this is fake :) we need a random number generator that exposes it's serializable state.
    let random =
        fun s (r: unit) ->
            let dotnetRandom = Option.defaultWith (fun () -> Random()) s
            { value = dotnetRandom.NextDouble()
              state = dotnetRandom }
        |> Gen

    let countFrom seed increment =
        fun s (r: unit) ->
            let state = Option.defaultValue (seed - 1) s
            let newValue = state + increment
            { value = newValue; state = newValue }
        |> Gen

    let count = countFrom 0 1

    let ifBang condition generator defaultValue =
        fun state (reader: unit) ->
            let lastXValue, lastXState = Option.defaultValue (None, None) state
            let xValue, xState =
                if condition then
                    let f = generator |> Gen.run
                    let res = f lastXState reader
                    Some res.value, Some res.state
                else
                    lastXValue, lastXState
            let newValue = Option.defaultValue defaultValue xValue
            { value = newValue; state = (xValue, xState) }
        |> Gen

    let ( <?> ) = ifBang
    let ( <!> ) = ( <| )

    let resume getReader state generator =
        let f = Gen.run generator
        let mutable state = Some state
        seq {
            while true do
                let res = f state (getReader())
                state <- Some res.state
                res
        }



let monteCarlo =
    0 <|> fun lastInsideCount (_: unit) -> gen {
        let! samples = countFrom 1 1
        let! x = random
        let! y = random
        let distance = Math.Sqrt (x*x + y*y)
        let isInsideCircle = distance < 1.0
        // let! insideCount = isInsideCircle <?> count <!> lastInsideCount
        let insideCount = if isInsideCircle then lastInsideCount + 1 else lastInsideCount
        let pi = 4.0 * float insideCount / float samples
        return { value = pi; state = insideCount }
    }

#time

// evaluate pi
let piSeq = monteCarlo |> Eval.Gen.toSeq2 ignore
let { value = pi; state = state } = piSeq |> Seq.take 1_000_000 |> Seq.last

// you can store state somewhere...
// load it and resume (we take only 1 additional sample and still get close to pi):
let resumedSeq = resume ignore state monteCarlo
let { value = piResumed; state = stateResumed } = piSeq |> Seq.take 1 |> Seq.last

// ...or see all n values (also resumable)
let piSeq2 = monteCarlo |> Eval.Gen.toSeq ignore
let values = piSeq2 |> Seq.take 1000 |> Seq.toList


let piSeq3 = monteCarlo |> Eval.Gen.toSeq ignore
let result = piSeq3 |> Seq.take 10_000_000 |> Seq.last
