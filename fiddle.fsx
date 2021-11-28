#r "./src/FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
open FsLocalState.Lib.Gen


loop {
    let! v1 = count 0 1     // count from 0, increment by 1
    let! v2 = count 100 5   // count from 1000, increment by 5
    yield v1 + v2
}
|> Gen.toListn 4

loop {
    let! v1 = [ "a"; "b"; "c"; "d" ] |> Gen.ofListOneByOne
    let! v2 = [  1 ;  2 ;  3 ;  4  ] |> Gen.ofListOneByOne
    yield v1,v2
}
|> Gen.toList


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


feed {
    // Place 'Init' on top of the computation and
    // give it a seed value (here: an empty list).
    // In the first evaluation, the seed value will be returned,
    // but in subsequent evaluations, the 'newState' value will be returned.
    let! state = Init []
    
    let! v = count 0 1
    let accumulatedValues = v :: state
    
    let output = accumulatedValues |> List.rev
    let newState = accumulatedValues
    
    // yield a tuple of the actual return value and a state value.
    yield output, newState
}
|> Gen.toListn 4
