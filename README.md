
﻿FsLocalState
===

FsLocalState is library designed to write and compose functions that preserve state from evaluation to the next.
This sounds like dealing with impure functions, but that's not the case.

A composable FsLocalState function takes a state as input and returns a value + state as output. The way those
functions are composed accumulates the output states of all functions inside of a computation, unpacks it and feeds it to the
corresponsing function in the next evaluation cycle.

The concept is based on my original work for a DSP / audio signal processing library in F#. You can read the
[article](http://schlenkr.binarygears.de/01_fsharp_dsp/01_Introduction.html) or have a look at the WIP repos
[here](https://github.com/ronaldschlenker/FluX) or [here](https://github.com/ronaldschlenker/compost). I find the library
useful when you have computations that deal with values over time, which is for example:

- Audio and video signal processing (DSP), where you compose filters, delays, effects and generators.
- Apply a set of rules over a (continuous) data series, like: "Signal me when a threshold is reached 3 times in the last 5 minutes".


Tutorial
---

### Loading the library

```fsharp
#r "../lib/FsLocalState.dll"

open FsLocalState
```

### Generators and Effects

Generator functions are the core part of FsLocalState. They are represented by the `Gen<'value, 'state, 'reader>` type.
Generators are in fact functions that have a state as input and a (value * state) as output:


                        +-------------+
                        |             |
                        |             +---------> 'value
          'reader +---->|     Gen     |
           'state +---->+             +-----+
                  |     |             |     |
                  |     +-------------+     |
                  |                         |
                  |                         |
                  +-------------------------+


A simple example of a generator is a counter:

```fsharp
// A generator with "seed" as input
let counter seed =
    
    // The generator function
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
```

#### Evaluation

We can now transform our counter function to a sequence that can be evaluated:

```fsharp
// (pass 'ignore' (fun i -> ()) to Eval.Gen.toEvaluableV to construct a reader value for each evaluation cycle)
let counterEval = counter 0 |> Eval.Gen.toEvaluableV ignore

// [0; 1; 2; 3; 4; 5; 6; 7; 8; 9]
let ``numbers from 0 to 9`` = counterEval 10
```

The generated sequence is an `IEnumerable<'a>`, which means:
We can continue pulling from 'counterSeq' and get the next (potentially different) results:

```fsharp
// [10; 11; 12; 13; 14; 15; 16; 17; 18; 19]
let ``numbers from 10 to 19`` = counterEval 10

// [20; 21; 22; 23; 24; 25; 26; 27; 28; 29]
let ``numbers from 20 to 29`` = counterEval 10
```

Note that:

- Generator functions themselves have only a state as input and have the signature: `'state option -> 'reader -> Res<'value, 'state>` (`'reader` is unused in these example).
- The generator function can be wrapped inside another function that takes input parameters.
- The characteristics of these parameters can be const-like (like *seed* above), or it can be a value that gets transformed. Technically, there is no
  difference between any kind of parameter. Wrapper functions with one (unsupplied) input parameters are called *Effect*, and we will see later that
  effects can also be composed easily.
- The `'reader` value is unused in this example, but can be useful when evaluating to pass in context from the runtime environment.
- The first evaluation of `counterEval 10` is equivalent to `seq { 0..9 }`


#### Init Comprehension

There is the `init` function that simplifies construction of `Gen` functions: Instead of dealing with initial state optionality inside the computation function,
you can specify a seed and pass it to the `init` function alongside with your computation function:

```fsharp
let counter2 seed =
    fun state (env: unit) ->
        let nextValue = state + 1
        { value = state; state = nextValue }
    |> Gen.init seed
```

### Compositon

Composing stateful functions is a key feature of FsLocalState. Before we look at ways of composition, we need another example to play with:

An *accumulator* that takes a "window" of the last n input values and sums them up:

```fsharp
let inline accu windowSize (input: 'a) =
    fun state (env: unit) ->
        let state = (input :: state) |> List.truncate windowSize
        let newValue = state |> List.sum
        { value = newValue; state = state }
    |> Gen.init []
```

You see that the `accu` function has 2 input parameters: `windowSize` determines how many past values should be summed up, and
`input` is a value coming in that gets summed up.

Let's see how it works:

```fsharp
let accuEval = accu 3 |> Eval.Eff.toEvaluableV ignore

// [1; 6; 8; 13; 21; 29]
let accuValues = [ 1; 5; 2; 6; 13; 10 ] |> accuEval
```

Note that in contrast to the `counter` function (which was a generator with no inputs) we here only apply the `windowSize`
parameter. What remains is an *Effect* function of type `'input -> Gen<...>`. This means:
 
- When we evaluate a generator, we use `Eval.Gen.toEvaluableV ` and pass the number of desired output values when evaluating.  
- When we evaluate an effect, we use `Eval.Eff.toEvaluableV ` and pass a sequence of input values.

### Kleisli Composition

Imagine you want to count values and phase the output:

                         +-------------+                               +-------------+
                         |             |                               |             |
          seed +-------->+             +--------->   (>=>)   +-------->+             +---------> output
                         |   counter   |                               |    accu     |
                   +---->+             +-----+                   +---->+             +-----+
                   |     |             |     |                   |     |             |     |
                   |     +-------------+     |                   |     +-------------+     |
                   |                         |                   |                         |
                   |                         |                   |                         |
                   +-------------------------+                   +-------------------------+
                                                                                           
                                                    -------
                                                    becomes
                                                    ------- 
                                                                                           
                         +-----------------------------------------------------------+
                         |                                                           |
          seed +-------->+                                                           +---------> output
                         |                      counter  accu                        |
                   +---->+                                                           +-----+
                   |     |                                                           |     |
                   |     +-----------------------------------------------------------+     |
                   |                                                                       |
                   |                                                                       |
                   +-----------------------------------------------------------------------+

Similar to composing "normal" functions by using "forward composition" operator (`>>`), we can compose
*Effect* functions by using the "Kleisli" operator `>=>`:

```fsharp
let accuCounter = counter >=> accu 3
let accuCounterResults =
    let seed = 0
    accuCounter |> Eval.Eff.toEvaluableV ignore <| Seq.replicate 10 seed
```

This works, but evaluating by passing a replicated sequence looks a bit weired because we treat the `seed` parameter of `counter` as a changing input, although it has
the character of a constant. We can change this by using the *pipe forward* (`|>`) equivalent: `|=>`:

```fsharp
let accuCounter2 = counter 0 |=> accu 3
let accuCounterResults2 = accuCounter2 |> Eval.Gen.toEvaluableV ignore <| 10
```

### Composition (Monad)

TODO

                         +-------------+                               +-------------+
                         |             |                               |             |
      input(s) +-------->+             +--------- ('counted') -------->+             +---------> output
                         |   counter   |                               |   phaser    |
                   +---->+             +-----+                   +---->+             +-----+
                   |     |             |     |                   |     |             |     |
                   |     +-------------+     |                   |     +-------------+     |
                   |                         |                   |                         |
                   |                         |                   |                         |
                   +-------------------------+                   +-------------------------+


```fsharp
let phasedCounter2 amount =
    gen {
        let! counted = counter
        let! phased = phaser amount (float counted)
        return phased
    }


```

// TODO

// TODO

// TODO
toEvaluable / toEvaluableV
State + Value oder nur Value

// TODO: State erklären
// Value restriction

// for what good is "reader state"? (use case)

conditional evaluation
for loops


```fsharp