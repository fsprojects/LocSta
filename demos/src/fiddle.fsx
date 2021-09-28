
#r "../../src/FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
#load "../../src/FsLocalState.Tests/testHelper.fs"

open System
open FsLocalState
open FsLocalState.Tests


let demo (seed: int) = feedback seed {
    //let! state = locals ()
    let! apples1,apples2 = Gen.countFrom 0 1
    let res = 1,1
    return res
}
