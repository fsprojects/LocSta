
﻿FsLocalState
===

![build status](https://github.com/ronaldschlenker/FsLocalState/actions/workflows/build_onPushMaster_onPullrequest.yml/badge.svg?event=push) ![test status](https://github.com/ronaldschlenker/FsLocalState/actions/workflows/test_onPushMaster_onPullrequest.yml/badge.svg?event=push)

FsLocalState is about

> Composing 'state-aware' functions as if they were just normal functions.

What is state(ful) - from a programmers perspective?

* State is a value that changes over time, so it *mutates*, and without mutation, it's not called state.
* Usually, (local) state is represented by an instance of an object or a closure that captures a mutable value.
* State must then have an 'identity', which is represented by a persistent location in memory - a pointer.
* In order to use state from a computation, it means: It has to be created *up-front*, and it can be used *afterwards* by it's pointer.
* The separation between *allocation* and *usage* makes composition hard, or at least uncomfortable (besides other thinks that make compotion hard or uncomfortable).

What is stateless?

* It's the opposite of stateful, which means:
* There is no mutation involved.
* No need for objects, state capturing closures or pointers - there are just functions.
* There is no persistence from one evaluation of a stateless computation to another.
* No separation between *allocation* and *usage*, which is comfortable to write: A function can be used in the place where it should be used - there is no de-localization in code between state allocation and usage, because there is no such thing as state allocation.

FsLocalState aims to providing comfort for compo a way for treating stateful computations like if they were stateless, giving 

FsLocalState is library designed to write and compose functions, where each of these functions inside of a computation
preserves it's own state from one evaluation to the next.

Composing 

While this might sound like dealing with impure or internally mutable functions, it is based on a pure function approach. The focus lies on dealing with sequences of values. Even though many concepts overlap with `seq`, there are significant differences in behaviour and usage, as well as in the fundamental ideas.

## Basic Examples

```fsharp
#r "./src/FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
open FsLocalState.Lib.Gen
```


**Count 2 values (use "count" as a stateful function)**

* While `count` seems to be a pure function (there's no object instance or pointer to an object), it is stateful per se.
* How does it work: The "Local State" CE evaluates the `count` functions, collects their state, and applies that state to the `count` functions again on subsequent evaluations.

```fsharp
loop {
    let! v1 = count 0 1     // count from 0, increment by 1
    let! v2 = count 100 5   // count from 1000, increment by 5
    yield v1 + v2
}
|> Gen.toListn 4

// [100; 106; 112; 118]
```


**"Pairwise" sequence processing**

* The *pairwise* characteristics seen in the example above can also be applied to sequences.
* Using the usual `seq` CE, the result woule be a cartesian product of sequence 1 and sequence 2

```fsharp
loop {
    let! v1 = [ "a"; "b"; "c"; "d" ] |> Gen.ofList
    let! v2 = [  1 ;  2 ;  3 ;  4  ] |> Gen.ofList
    yield v1,v2
}
|> Gen.toList

// [("a", 1); ("b", 2); ("c", 3); ("d", 4)]
```


**Controlling**

It is possible to explicitly control the workflow by emitting `Stop`, `Skip` and others:

```fsharp
loop {
    let! v = count 0 1      // this would yield and never stop
    if v = 5 then
        return Loop.Skip    // we don't want '5' to be part of the result
    elif v = 10 then
        return Loop.Stop    // 'break' after 10 elements are yielded
    else
        yield v
}
|> Gen.toList

// [0; 1; 2; 3; 4; (* no 5 in here *) 6; 7; 8; 9]
```


**Writing stateful functions**

* Until now, we only *used* stateful functions. But what if we want to *write* functions, and maintain a state?
* Here's an example: Accumulate counted values, so that in each evaluation cycle, a list with all the counted values since beginning is yielded.
* Instead of the `loop` builder, it uses the `feed` builder.

```fsharp
feed {
    let! state = Init []        // Place 'Init' on top of the computation and
                                // give it a seed value (here: an empty list).
                                // In the first evaluation, the seed value
                                // will be returned, but in subsequent evaluations,
                                // the 'newState' value will be returned.
    
    let! v = count 0 1
    let accumulatedValues = v :: state
    
    let output = accumulatedValues |> List.rev
    let newState = accumulatedValues
    
    yield output, newState      // yield a tuple of the actual return value
                                // and a state value.
}
|> Gen.toListn 4

//    [
//        [0]
//        [0; 1]
//        [0; 1; 2]
//        [0; 1; 2; 3]
//    ]
```

## More Examples and Documentation

**Tests**

Please have a look at [the base tests](./src/FsLocalState.Tests) for getting an impression of how the library works and maybe what it could be good for.

**Previous versions**

https://github.com/ronaldschlenker/FsLocalState/tree/2360afbc45f646338e725b8059943dff3d41c5af

**F# Applied Challenge**

The concept was also part of the F# Applied Challenge in 2019. Have a look ar the [contribution explanation](https://github.com/ronaldschlenker/applied_fsharp_challenge/tree/master/output/_htmlOutput) to find out more.



# Current State of Development

*This library is still experimental and volatile.*
