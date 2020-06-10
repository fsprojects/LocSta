FsLocalState
===

This article demonstrates how to use the FsLocalState library.

TODO: Allgemein erklären, für was die Library gut ist. 
TODO: Link to article

Loading the library
---

```fsharp
#r "../lib/FsLocalState.dll"

open FsLocalState
open FsLocalState.Eval
```

Usage
---

### Generators

Generators are functions that have only a state as input and a (value * state) as output.


                   Generator
                +-------------+
                |             |
                |             +---------> output
                |   counter   |
    state +---->+             +-----+
          |     |             |     |
          |     +-------------+     |
          |                         |
          |                         |
          +-------------------------+


A simple example of a generator is a counter:

```fsharp
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
```

Note that generator functions (as well as effect functions that take input parameters) have the signature:

`'state option -> 'reader -> Res<'value, 'state>`

The `'reader` value is unused in this example, but can be useful when evaluating to pass in context from
the runtime environment.

#### Evaluation

We can now transform our counter function to a sequence that can be evaluated:

```fsharp
// (pass 'ignore' (fun i -> ()) to Gen.toEvaluableValues to construct a reader value for each evaluation cycle)
let counterEval = counter |> Gen.toEvaluableValues (fun i -> ())

// [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let ``numbers from 1 to 10`` = counterEval 10
```

The generated sequence is an IEnumerable<>, which is a state machine. It's not idempotent, which means:
We can continue pulling from 'counterSeq' to get the next (potentially different) results:

```fsharp
// [11; 12; 13; 14; 15; 16; 17; 18; 19; 20]
let ``numbers from 11 to 20`` = counterEval 10
```


#### Init comprehension

There is the `init` function that simplifies construction of `Local` functions:

```fsharp
let counter' =
    fun state (env: unit) ->
        let newValue = state + 1
        { value = newValue
          state = newValue }
    |> init 0
```


### Effects

Effects are functions that returns an inner generator function after all input parameters are applied.


                           Effect
                       +-------------+
                       |             |
    input(s) +-------->+             +---------> output
                       | slowCounter |
           state +---->+             +-----+
                 |     |             |     |
                 |     +-------------+     |
                 |                         |
                 |                         |
                 +-------------------------+


As an example of an effect, we implement a phaser that takes an input value and adds a fraction of the last input:

```fsharp
let phaser amount (input: float) =
    fun state (env: unit) ->
        let newValue = input + state * amount
        { value = newValue
          state = input }
    |> init 0.0
```


You see that the `phase` function has 2 input parameters: `amount` is a constant value and `input` is passed
as a sequence of values when evaluating the effect. How an input parameter is treated (constant, changing) is
only based on the way your function is used, not how it is designed.

#### Evaluation

We can now transform our counter function to a sequence that can be evaluated:

```fsharp
let phaserAmount = 0.1
let phaserEval =
    phaser phaserAmount
    |> Fx.toEvaluableValues (fun i -> ())

// [1.0; 2.1; 3.2; 4.3]
let phasedValues =
    let inputValues = [ 1.0; 2.0; 3.0; 4.0 ]
    phaserEval inputValues
```


### Composition (Monad)

Composing stateful functions is a key feature of FsLocalState:

Imagine you want to count values and phase the output:

                         +-------------+                               +-------------+
                         |             |                               |             |
      input(s) +-------->+             +--------- ('counted') -------->+             +---------> output
                         |      f      |                               |      g      |
                   +---->+             +-----+                   +---->+             +-----+
                   |     |             |     |                   |     |             |     |
                   |     +-------------+     |                   |     +-------------+     |
                   |                         |                   |                         |
                   |                         |                   |                         |
                   +-------------------------+                   +-------------------------+

```fsharp
let phasedCounter amount =
    local {
        let! counted = counter
        let! phased = phaser amount (float counted)
        return phased
    }
```

We can evaluate this network (here, we have no input value, so the `phasedCounter` is a generator):

```fsharp
let phasedCounterAmount = 0.1
let phasedCounterEval =
    phasedCounter phasedCounterAmount
    |> Gen.toEvaluableValues (fun i -> ())

// [1.0; 2.1; 3.2; 4.3; 5.4; 6.5; 7.6; 8.7; 9.8; 10.9]
let phasedCounterValues = phasedCounterEval 10
```


### Sequential Compositon (Kleisli)

TODO

                         +-------------+                               +-------------+
                         |             |                               |             |
      input(s) +-------->+             +--------->    >=>    +-------->+             +---------> output
                         |      f      |                               |      g      |
                   +---->+             +-----+                   +---->+             +-----+
                   |     |             |     |                   |     |             |     |
                   |     +-------------+     |                   |     +-------------+     |
                   |                         |                   |                         |
                   |                         |                   |                         |
                   +-------------------------+                   +-------------------------+