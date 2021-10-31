
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

type While<'v> =
    { guard: unit -> bool
      body: unit -> 'v }

type Gen.GenBuilder with
    member this.Delay(f) = 
        printfn "DELAY %A" f
        f
    member this.Run(f) = 
        printfn "RUN %A" f
        f()
    member this.Yield(x) = x
    member this.While (guard, body) =
        if not (guard()) then
            Gen.ofResult Stop
        else
            let res = body()
            Gen.ofResult (Value(res, { guard = guard; body = body }))

let count start inc max =
    gen {
        printfn "Start"
        let mutable curr = start
        while curr < max do
            printfn "Loop"
            let! a = count_0_1
            curr <- curr + inc
            yield curr
    }

let res = gen {
    let! c1 = count 0 1 10
    let! c2 = count_0_1
    return Value(c1 + c2, ())
}

res |> Gen.toListn 15

