﻿FsLocalState
===

This article demonstrates how to use the FsLocalState library.

TODO: Allgemein erklären, für was die Library gut ist. 
TODO: Link to article

Loading the library
---

$ref: loadingLibrary

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

$ref: generatorSample

Note that generator functions (as well as effect functions that take input parameters) have the signature:

`'state option -> 'reader -> Res<'value, 'state>`

The `'reader` value is unused in this example, but can be useful when evaluating to pass in context from
the runtime environment.

#### Evaluation

We can now transform our counter function to a sequence that can be evaluated:

$ref: generatorEval1

The generated sequence is an IEnumerable<>, which is a state machine. It's not idempotent, which means:
We can continue pulling from 'counterSeq' to get the next (potentially different) results:

$ref: generatorEval2


#### Init comprehension

There is the `init` function that simplifies construction of `Local` functions:

$ref: initComprehension


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

$ref: effectSample


#### Evaluation

We can now transform our counter function to a sequence that can be evaluated:

$ref: effectEval1

