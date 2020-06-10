

//$ref: loadingLibrary
#r "../lib/FsLocalState.dll"

open FsLocalState
open FsLocalState.Eval



//$ref: generatorSample
let counter =
    fun state (env: unit) ->

        // The first time our function is evaluated, there is no state ('state' parameter is an option).
        // So we need an initial value for the counter:
        let seed = 0

        let state =
            match state with
            | None -> seed
            | Some x -> x

        // calculate the counter value
        let newValue = state + 1

        // always return value and state.
        { value = newValue
          state = newValue }
    |> Local


//$ref: generatorEval1
// (pass 'ignore' (fun i -> ()) to Gen.toEvaluableValues to construct a reader value for each evaluation cycle)
let doCount = counter |> Gen.toEvaluableValues (fun i -> ())

// [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let ``numbers from 1 to 10`` = doCount 10


//$ref: generatorEval2
// [11; 12; 13; 14; 15; 16; 17; 18; 19; 20]
let ``numbers from 11 to 20`` = doCount 10


//$ref: initComprehension
let counter' =
    fun state (env: unit) ->
        let newValue = state + 1
        { value = newValue
          state = newValue }
    |> init 0


//$ref: effectSample
let slowCounter (amount: float) =
    fun state (env: unit) ->

        // calculate the counter value
        let newValue = state + 1.0

        // always return value and state.
        { value = newValue
          state = newValue }
    |> init 0.0



//$ref: effectEval1
let doCountSlow = slowCounter |> Fx.toEvaluableValues (fun i -> ())

// [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let ``TODO`` =
    let constantAmount = 0.2
    doCountSlow (Seq.replicate 10 constantAmount)

