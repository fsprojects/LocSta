

//$ref: loadingLibrary
#r "../lib/FsLocalState.dll"

open FsLocalState

//$ref: generatorSample
let counter seed =
    fun state (env: unit) ->
        let state =
            match state with
            | None -> seed
            | Some x -> x

        // calculate the value for the next cycle
        let nextValue = state + 1

        // always return value and state.
        { value = state; state = nextValue }
    |> Gen


//$ref: generatorEval1
// (pass 'ignore' (fun i -> ()) to Eval.Gen.toEvaluableV to construct a reader value for each evaluation cycle)
let counterEval = counter 0 |> Eval.Gen.toEvaluableV ignore

// [0; 1; 2; 3; 4; 5; 6; 7; 8; 9]
let ``numbers from 0 to 9`` = counterEval 10


//$ref: generatorEval2
// [10; 11; 12; 13; 14; 15; 16; 17; 18; 19]
let ``numbers from 10 to 19`` = counterEval 10

// [20; 21; 22; 23; 24; 25; 26; 27; 28; 29]
let ``numbers from 20 to 29`` = counterEval 10


//$ref: initComprehension
let counter2 seed =
    fun state (env: unit) ->
        let nextValue = state + 1
        { value = state; state = nextValue }
    |> Gen.init seed


//$ref: effectSample
let inline accu windowSize (input: 'a) =
    fun state (env: unit) ->
        let state = (input :: state) |> List.truncate windowSize
        let newValue = state |> List.sum
        { value = newValue; state = state }
    |> Gen.init []

//$ref: effectEval1
let accuEval = accu 3 |> Eval.Eff.toEvaluableV ignore

// [1; 6; 8; 13; 21; 29]
let accuValues = [ 1; 5; 2; 6; 13; 10] |> accuEval




//$ref: kleisliPipeSample1
let phasedCounter = counterFloat |=> phaser 0.1

//$ref: mapAndKleisliPipeSample1
let phasedCounterFinal =
    counter
    <!> (fun x -> float x)
    |=> phaser 0.1

//$ref: compositionKleisliSample1
let phasedTwice = phaser 0.3 >=> phaser 0.1

//$ref: mapSample1
let counterFloat = counter |> Gen.map (fun x -> float x)

//$ref: mapSample2
let counterFloat2 = counter <!> fun x -> float x




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


