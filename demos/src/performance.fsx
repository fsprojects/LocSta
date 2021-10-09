
#r "../../src/FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"

open FsLocalState

let dummy1 =
    fun s ->
        let s = Option.defaultValue 0 s
        let v = s + 1
        Some (v, v)
    |> Gen.create

let dummy2 =
    0
    |> Gen.ofSeed (fun s ->
        let v = s + 1
        Some (v, v)
    )

let dummy3 =
    0 => fun state -> gen {
        let v = state + 1
        return v, v
    }

let test1 =
    gen {
        let! a = dummy1
        let! b = dummy1
        let! c = dummy1
        return ()
    }

let test2 =
    gen {
        let! a = dummy2
        let! b = dummy2
        let! c = dummy2
        return ()
    }

let test3 =
    gen {
        let! a = dummy3
        let! b = dummy3
        let! c = dummy3
        return ()
    }

#time

// evaluate pi
let res1 = test1 |> Gen.toSeq |> Seq.take 5_000_000 |> Seq.last
let res2 = test2 |> Gen.toSeq |> Seq.take 5_000_000 |> Seq.last
let res3 = test3 |> Gen.toSeq |> Seq.take 5_000_000 |> Seq.last
