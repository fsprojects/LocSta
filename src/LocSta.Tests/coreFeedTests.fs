#if INTERACTIVE
#r "../LocSta/bin/Debug/netstandard2.0/LocSta.dll"
open LocSta
#endif

module ``Core Tests (Feed)``

open TestHelper
open LocSta
open LocSta.Lib
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

let [<TestCase>] ``Feed.Stop`` () =
    feed {
        let! v = Init 0
        let nextValue = v + 1
        if v < 5 then
            yield v, nextValue
        else
            return Feed.Stop
    }
    |> Gen.toList
    |> equals [ 0 .. 4 ]
