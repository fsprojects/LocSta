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
let doCount = counter |> Gen.toEvaluableValues (fun i -> ())

// [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let ``numbers from 1 to 10`` = doCount 10
```

The generated sequence is an IEnumerable<>, which is a state machine. It's not idempotent, which means:
We can continue pulling from 'counterSeq' to get the next (potentially different) results:

```fsharp
// [11; 12; 13; 14; 15; 16; 17; 18; 19; 20]
let ``numbers from 11 to 20`` = doCount 10
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


As an example of an effect, we implement a counter that subtracts a certain 'amount' (specified by an input)
of the last counter value:

```fsharp
let slowCounter (amount: float) =
    fun state (env: unit) ->

        // calculate the counter value
        let newValue = state + 1.0

        // always return value and state.
        { value = newValue
          state = newValue }
    |> init 0.0
```


#### Evaluation

We can now transform our counter function to a sequence that can be evaluated:

```fsharp
let doCountSlow = slowCounter |> Fx.toEvaluableValues (fun i -> ())

// [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
let ``TODO`` =
    let constantAmount = 0.2
    doCountSlow (Seq.replicate 10 constantAmount)
```
