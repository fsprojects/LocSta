

//$ref: loadingLibrary
#r "../lib/FsLocalState.dll"

open FsLocalState


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
        { value = newValue; state = newValue }
    |> Gen


//$ref: generatorEval1
// (pass 'ignore' (fun i -> ()) to Eval.Gen.toEvaluableV to construct a reader value for each evaluation cycle)
let counterEval = counter |> Eval.Gen.toEvaluableV ignore

// [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let ``numbers from 1 to 10`` = counterEval 10


//$ref: generatorEval2
// [11; 12; 13; 14; 15; 16; 17; 18; 19; 20]
let ``numbers from 11 to 20`` = counterEval 10

// [21; 22; 23; 24; 25; 26; 27; 28; 29; 30]
let ``numbers from 21 to 30`` = counterEval 10


//$ref: initComprehension
let counter2 =
    fun state (env: unit) ->
        let newValue = state + 1
        { value = newValue; state = newValue }
    |> Gen.init 0






//$ref: effectSample
let phaser amount (input: float) =
    fun state (env: unit) ->
        let newValue = input + state * amount
        { value = newValue; state = input }
    |> Gen.init 0.0

//$ref: effectEval1
let phaserEval =
    let phaserAmount = 0.1
    phaser phaserAmount |> Eval.Eff.toEvaluableV ignore

// [1.0; 2.1; 3.2; 4.3]
let phasedValues = [ 1.0; 2.0; 3.0; 4.0 ] |> phaserEval




//$ref: mapSample1
let counterFloat = counter |> Gen.map (fun x -> float x)

//$ref: mapSample2
let counterFloat2 = counter <!> fun x -> float x

//$ref: kleisliPipeSample1
let phasedCounter = counterFloat |=> phaser 0.1

//$ref: mapAndKleisliPipeSample1
let phasedCounterFinal =
    counter
    <!> (fun x -> float x)
    |=> phaser 0.1

//$ref: compositionKleisliSample1
let phasedTwice = phaser 0.3 >=> phaser 0.1





//$ref: compositionMonadSample
let phasedCounter2 amount =
    gen {
        let! counted = counter
        let! phased = phaser amount (float counted)
        return phased
    }




//$ref: UNUSED

//
// let fold n =
//     
// let x = phaser 0.0 >=> phaser 0.1
// x
// |> Fx.toEvaluableValues (fun i -> ())
// <| [ 1.0; 2.0; 3.0; 4.0 ]


