

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
let counterEval = counter |> Gen.toEvaluableValues (fun i -> ())

// [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let ``numbers from 1 to 10`` = counterEval 10


//$ref: generatorEval2
// [11; 12; 13; 14; 15; 16; 17; 18; 19; 20]
let ``numbers from 11 to 20`` = counterEval 10


//$ref: initComprehension
let counter' =
    fun state (env: unit) ->
        let newValue = state + 1
        { value = newValue
          state = newValue }
    |> init 0






//$ref: effectSample
let phaser amount (input: float) =
    fun state (env: unit) ->
        let newValue = input + state * amount
        { value = newValue
          state = input }
    |> init 0.0

//$ref: effectEval1
let phaserAmount = 0.1
let phaserEval =
    phaser phaserAmount
    |> Fx.toEvaluableValues (fun i -> ())

// [1.0; 2.1; 3.2; 4.3]
let phasedValues =
    let inputValues = [ 1.0; 2.0; 3.0; 4.0 ]
    phaserEval inputValues




//$ref: compositionMonadSample
let phasedCounter amount =
    local {
        let! counted = counter
        let! phased = phaser amount (float counted)
        return phased
    }

//$ref: compositionMonadEval
let phasedCounterAmount = 0.1
let phasedCounterEval =
    phasedCounter phasedCounterAmount
    |> Gen.toEvaluableValues (fun i -> ())

// [1.0; 2.1; 3.2; 4.3; 5.4; 6.5; 7.6; 8.7; 9.8; 10.9]
let phasedCounterValues = phasedCounterEval 10
