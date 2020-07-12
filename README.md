
﻿FsLocalState
===

FsLocalState is library designed to write and compose functions, where each of these functions inside of a computation
preserves it's own state from one evaluation to the next. While this might sound like dealing with impure or internally mutable functions -
which would have object semantic and thus making composability hard or impossible - it is based on a pure function approach.

```fsharp
let mySynthVoice frq =
    gen {
        let! modulator = sin 5.0
        let amount = 0.05
        let! res =
            saw (frq * (1.0 - modulator * amount))
            |=> lowpass 12000.0
            |=> highpass 2000.0
        return res
    }
```


Try FsLocalState in your browser
---

Click this button to launch a Binder instance, where you can get plotting interactively!

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/ronaldschlenker/FsLocalState.Interactive/master)

The binder repo can be found [here](https://github.com/ronaldschlenker/FsLocalState.Interactive).

Quick Start
---

A composable FsLocalState function takes a state as input and returns a value + state as output. The composition mechanisms
provided by the library accumulate the output states of all functions inside of a computation, unpacks it and feeds it to the
corresponsing function in the next evaluation cycle.

Computations can then be treated as either

- *Generators: a sequence of values, represented by `seq<'output>`
- *Effects: a sequence of values, represented by `seq<'input> -> seq<'output>`

The concept is based on my original work for a DSP / audio signal processing library in F#. You can read the
[article](http://schlenkr.binarygears.de/01_fsharp_dsp/01_Introduction.html) or have a look at the WIP repos
[here](https://github.com/ronaldschlenker/FluX) or [here](https://github.com/ronaldschlenker/compost). I find the library
useful when you have computations that deal with values over time, which is for example:

- Audio and video signal processing (DSP), where you compose filters, delays, effects and generators.
- Apply a set of rules over a (continuous) data series, like: "Signal me when a threshold is reached 3 times in the last 5 minutes".

*Demos*

For further demos, have a look at the fsx files in `./demos/src`.

You have to run `paket install` in the `demos` folder to use the charting samples.

Tutorial
---

### Loading the library

```fsharp
#r "../lib/FsLocalState.dll"

open FsLocalState
open FsLocalState.Operators
```

### Generators and Effects

Generator functions are the core part of FsLocalState. They are represented by the `Gen<'value, 'state, 'reader>` type.
They that have a state as input and a (value * state) as output:


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
// A constructor for the generator with a "seed"
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
// (pass 'ignore' (fun i -> ()) to Gen.toSeq to construct a reader value for each evaluation cycle)
let counterEval = counter 0 |> Gen.toSeq ignore

let numbers_0_9 = counterEval |> Seq.toListN 10
    // res: [0; 1; 2; 3; 4; 5; 6; 7; 8; 9]
```

The generated sequence is an `IEnumerable<'a>`, which means:
We can continue pulling from 'counterSeq' and get the next (potentially different) results:

```fsharp
let numbers_10_19 = counterEval |> Seq.toListN 10
    // res: [10; 11; 12; 13; 14; 15; 16; 17; 18; 19]

let numbers_20_29 = counterEval |> Seq.toListN 10
    // res: [20; 21; 22; 23; 24; 25; 26; 27; 28; 29]
```

Note that:

- The `'reader` value is unused in this example, but can be useful when evaluating to pass in context from the runtime environment.
- The first evaluation of `counterEval 10` is equivalent to `seq { 0..9 }`



#### Effects

- Generator functions themselves have only a state as input.
  - They have the signature: `'state option -> 'reader -> Res<'value, 'state>`.
- The generator function can be wrapped inside another function (here called _constructor functions_) that takes one or more input parameters.
  - Here: We have a single `seed` parameter.
- The characteristics of these input parameters can be const-like (like *seed* above), or it can be a value that gets transformed.
  (in the sense of transforming an input value to an output value). Technically, there is no
  difference between those kinds. Constructor functions with one (unsupplied) input parameters are called *Effect*, and there are
  several supported use cases for them in the library.

As an example, we create an *accumulator*: It takes a "window" of the last n input values and sums them up, so that a value in
the output sequence is the sum of the last n values of the input sequence:


```fsharp
let inline accu windowSize (input: 'a) =
    fun state (env: unit) ->
        let state = (input :: state) |> List.truncate windowSize
        let newValue = state |> List.sum
        { value = newValue; state = state }
    |> Gen.initValue []
```

You see that the `accu` function has 2 input parameters: `windowSize` determines how many past values should be summed up, and
`input` is a value coming in that gets summed up.

Let's see how it works:

```fsharp
let accuEval = accu 3 |> Eff.toSeq ignore

let accuValues = [ 1; 5; 2; 6; 13; 10 ] |> accuEval
    // res: [1; 6; 8; 13; 21; 29]
```

Note that in contrast to the `counter` function (which was a generator with no inputs) we here only apply the `windowSize`
parameter. What remains is an *Effect* function of type `'input -> Gen<...>`. This means:
 
- When we evaluate a generator, we use `Gen.toSeq` and pass the number of desired output values when evaluating.  
- When we evaluate an effect, we use `Eff.toSeq` and pass a sequence of input values.





#### Init Comprehension

There is the `init` function that simplifies construction of `Gen` functions: Instead of dealing with initial state optionality inside the computation function,
you can specify a seed and pass it to the `init` function alongside with your computation function:

```fsharp
let counter2 seed =
    fun state (env: unit) ->
        let nextValue = state + 1
        { value = state; state = nextValue }
    |> Gen.initValue seed
```

### Compositon

Composing stateful functions is a key feature of FsLocalState and there are the "usual suspect" ways of composition:

#### Kleisli (serial) Composition

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
    Seq.replicate 10 seed |> Eff.toSeq ignore accuCounter 
```

This works, but evaluating by passing a replicated sequence looks a bit weired because we treat the `seed` parameter of `counter` as a changing input, although it has
the character of a constant. We can change this by using the *pipe forward* (`|>`) equivalent: `|=>`:

```fsharp
let accuCounter2 = counter 0 |=> accu 3
let accuCounterResults2 = accuCounter2 |> Gen.toSeq ignore |> Seq.toListN 10
```

#### Composition (Monad)

There is also a more flexible way of composition that 

                         +-------------+                                          +-------------+
                         |             |                          ...             |             |
          seed +-------->+             +--------- ('counted')     ...      ------>+             +---------> output
                         |   counter   |                          ...             |    accu     |
                   +---->+             +-----+           do whatever you    +---->+             +-----+
                   |     |             |     |           want with the      |     |             |     |
                   |     +-------------+     |           'counted' value    |     +-------------+     |
                   |                         |                    ...       |                         |
                   |                         |                              |                         |
                   +-------------------------+                              +-------------------------+


```fsharp
let accuCounter3 amount =
    gen {
        let! counted = counter 0
        let! output = accu 3 counted
        return output
    }

```

#### Feedback

When you are inside of a `gen` computation and need to feed a value "back to the future", you can also do that:
 
                       +-----------------------------------------------------------------------+
                       |                                                                       |
                       |             +--+                         +--+                         |
    input(s) +-------->+          +->+fa+--+                   +->+fc+--+                      +-----------> output
                       |          |  +--+  |                   |  +--+  |                      |
                       |          |        |                   |        |                      |
                       |          +--------+                   +--------+                      |
        feedback +---->+                          +--+                         +--+            |
                 |     |                       +->+fb+--+                   +->+fd+--+         |
                 |     |                       |  +--+  |                   |  +--+  |         |
                 |     |                       |        |                   |        |         +-----+
                 |     |                       +--------+                   +--------+         |     |
                 |     |                                                                       |     |
                 |     +-----------------------------------------------------------------------+     |
                 |                                                                                   |
                 |                                                                                   |
                 |                                                                                   |
                 |                                                                                   |
                 +-----------------------------------------------------------------------------------+

This can be done by using the feedback operator `<|>`:

```fsharp
let feedbackExample =
    let seed = 0
    seed <|> fun lastValueOfCounter1 env ->
        gen {
            let! counter1 = counter 0
            let! counter2 = counter 10
            
            // "state" will be available in the next evaluation (lastValueOfCounter1)
            return { value = if lastValueOfCounter1 >= 10 then 0 else counter1 + counter2
                     state = counter1 }
        }

feedbackExample |> Gen.toSeq ignore |> Seq.toListN 20
    // res: [10; 12; 14; 16; 18; 20; 22; 24; 26; 28; 30; 0; 0; 0; 0; 0; 0; 0; 0; 0]
```

### Reader State

An use case for reader state: Pass in a sample rate:

```fsharp
type Env =
    { sampleRateHz: int }
    
let sinOsc (frq: float) =
    let pi = System.Math.PI
    let pi2 = 2.0 * pi
    
    0.0 <|> fun angle (env: Env) ->
        gen {
            let newAngle = (angle + pi2 * frq / (float env.sampleRateHz)) % pi2
            return { value = System.Math.Sin newAngle
                     state = newAngle }
        }
```

### Arithmetik

Applying basic arithmetik functions on generators is also possible.

```fsharp
// Adding 2 gens.
gen {
    let! res = counter 0 + counter 10
    return res
}
|> Gen.toSeq ignore
|> Seq.toListN 10

// Adding a constant to a gen.
// Note: this is currently only possible for int and float.
gen {
    let! res = counter 0 + 10
    return res
}
|> Gen.toSeq ignore
|> Seq.toListN 10
```

### TODOs

- Map / Apply
- Explain signatures with accumulated state
- in addition to mySynth, make a sample using a data series rule


```fsharp