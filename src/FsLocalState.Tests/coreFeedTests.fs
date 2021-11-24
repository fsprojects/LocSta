#if INTERACTIVE
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState
#endif

module ``Core Tests (Feed)``

open TestHelper
open FsLocalState
open FsLocalState.Lib.Gen
open NUnit.Framework


// TODO: how model combine on feed?
//let [<TestCase>] ``TODO`` () =
//    feed {
//        let! state = Init 10
//        yield 5,state
        
//        let! state = Init 10
//        yield 5,state
//    }
//    |> Gen.toListn 10
//    |> equals [0;10]
    