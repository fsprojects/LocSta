FsLocalState
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
    (input(s) +------->)|             |
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
```


#### Evaluation

We can now transform our counter function to a sequence that can be evaluated:

```fsharp
// (pass 'ignore' (fun i -> ()) to Eval.Gen.toEvaluableV to construct a reader value for each evaluation cycle)
let counterEval = counter 0 |> Eval.Gen.toEvaluableV ignore

// [0; 1; 2; 3; 4; 5; 6; 7; 8; 9]
let ``numbers from 0 to 9`` = counterEval 10
```

The generated sequence is an IEnumerable<'a>, which means:
We can continue pulling from 'counterSeq' and get the next (potentially different) results:

```fsharp
// [10; 11; 12; 13; 14; 15; 16; 17; 18; 19]
let ``numbers from 10 to 19`` = counterEval 10

// [20; 21; 22; 23; 24; 25; 26; 27; 28; 29]
let ``numbers from 20 to 29`` = counterEval 10
```

Note that:

- Generator functions themselves have only a state as input and have the signature: `'state option -> 'reader -> Res<'value, 'state>` (`'reader` is unused in these example).
- The generator function can be wrapped inside another function that takes input parameters. After all input parameters are applied, the generator function remains.
  When these functions have only one input parameter (which can be tupled), we call it an *Effect*.
- The `'reader` value is unused in these example, but can be useful when evaluating to pass in context from the runtime environment.
- The first evaluation of `counterEval 10` is equivalent to `seq { 0..9 }`


#### Init comprehension

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

Before we look at ways of composing stateful functions, we need another example to play with:


An *accumulator* that takes an "window" of the last n input values and sums them up:

```fsharp
let inline accu windowSize (input: 'a) =
    fun state (env: unit) ->
        let state = (input :: state) |> List.truncate windowSize
        let newValue = state |> List.sum
        { value = newValue; state = state }
    |> Gen.init []
```

You see that the `accu` function has 2 input parameters: `windowSize` determines how many past values should be summed up, and
`input` is current value.

We can now transform our counter function to a sequence that can be evaluated:

```fsharp
let accuEval = accu 3 |> Eval.Eff.toEvaluableV ignore

// [1; 6; 8; 13; 21; 29]
let accuValues = [ 1; 5; 2; 6; 13; 10] |> accuEval
```

Note that in contrast to the `counter` function (which was a generator with no inputs) we here only apply the `windowSize`
parameter. What remains is a function of type `'input -> Gen<...>`. This means:
 
- When we evaluate a generator, we use `Eval.Gen` and pass the number of desired output values when evaluating.  
- When we evaluate an effect, we use `Eval.Eff` and pass a sequence of input values.


Composing stateful functions is a key feature of FsLocalState. Imagine you want to count values and phase the output:

                         +-------------+                               +-------------+
                         |             |                               |             |
      input(s) +-------->+             +--------->   (>=>)   +-------->+             +---------> output
                         |   counter   |                               |   phaser    |
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
      input(s) +-------->+                                                           +---------> output
                         |                      counter  phaser                      |
                   +---->+                                                           +-----+
                   |     |                                                           |     |
                   |     +-----------------------------------------------------------+     |
                   |                                                                       |
                   |                                                                       |
                   +-----------------------------------------------------------------------+

As we will see, this can be done in more than one way. But you might see that this looks like
the "forward composition" operator (`>>`). Since we have a "wrapper type" that cannot be composed using `>>`,
there comes the "Kleisli" operator `>=>` to rescue:


```fsharp
let phasedTwice = phaser 0.3 >=> phaser 0.1
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