
#r "../../src/LocSta/bin/Debug/netstandard2.0/LocSta.dll"

open LocSta

let withGenFunc =
    fun s ->
        let s = Option.defaultValue 0 s
        let v = s + 1
        Res.Loop.emit v v
    |> Gen.createLoop

let withFeed =
    feed {
        let! state = Init 0
        let v = state + 1
        yield v, v
    }

let testWithGenFunc =
    loop {
        let! a = withGenFunc
        let! b = withGenFunc
        let! c = withGenFunc
        yield ()
    }

let testWithFeed =
    loop {
        let! a = withFeed
        let! b = withFeed
        let! c = withFeed
        yield ()
    }

#time

// evaluate pi
let evalTest f = f |> Gen.toSeq |> Seq.take 5_000_000 |> Seq.last
let res1 = evalTest testWithGenFunc
let res2 = evalTest testWithFeed
