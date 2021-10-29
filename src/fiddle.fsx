
#r "FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

let count inclusiveStart increment =
    fun s ->
        let state = Option.defaultWith (fun () -> inclusiveStart - 1) s
        let newValue = state + increment
        Value (newValue, newValue)
    |> Gen.create

let test =
    gen {
        let! c1 = count 0 1
        let! c2 = count 0 2
        let! v1 = 0 => (fun state -> gen {
            let! c11 = count 0 1
            let! c21 = count 0 2
            return (c11 + c21, state)
        })
        return c1 + c2 + v1
    }


let test2 =
    count 0 1 |> Gen.bind (fun c1 ->
    count 0 2 |> Gen.bind (fun c2 ->
    (Gen.feedback 0 (fun state ->
        count 0 1 |> Gen.bind (fun c11 ->
        count 0 2 |> Gen.bind (fun c21 ->
        Gen.ofValue (c11 + c21, state)
    ))))      |> Gen.bind (fun v1 ->
    Gen.ofValue (c1 + c2 + v1)
)))



let test3 =
    count 0 1 |> Gen.bind (fun c1 ->
    count 0 2 |> Gen.bind (fun c2 ->
    (Gen.feedback 0 (fun state ->
        count 0 1 |> Gen.bind (fun c11 ->
        count 0 2 |> Gen.bind (fun c21 ->
        Gen.ofValue (c11 + c21, state)
    ))))      |> Gen.bind (fun v1 ->
    Gen.ofValue (c1 + c2 + v1)
)))


module Z =
    let counter seed inc =
        gen {
            let mutable curr = seed
            while true do
                curr <- curr + inc
                yield curr
        }


module A =
    type T =
        | Int of int
        | String of string

    type Builder() =
        member this.Yield(x: string) = String x
        member this.Yield(x: int) = Int x
        member this.Combine(a, b) = [a;b]
        member this.Delay(f) = f()

    let b = Builder()

    b {
        if 1 = 1 then
            yield "Ökjklö"
        else
            yield 24
    }


/////////////////
/////////////////
/////////////////
/////////////////



module B =
    type Box<'v, 's> =
        | Box of ('v * 's)
    type InitWith<'s> = 's
    type InitLater<'s, 'v> = ('s -> 'v)

    type ScopeBuilder() =
        member this.Bind(m: Box<_,_>, f) = 
            let (Box b) = m
            let value,state = b
            let fRes = f value
            Box (fRes, state)
        member this.Bind(m: InitWith<_>, f) =
            let box = Box (m, m)
            this.Bind (box, f)
        member this.Bind(m: InitLater<_,_>, f) =
            fun s ->
                let box = Box (s, s)
                this.Bind (box, f)
        member this.Return(x) = x

    let scope = ScopeBuilder()

    let initWith value = InitWith value
    let initLater<'a> = (fun s -> s) |> InitLater

    let a = scope {
        let! state = initWith "a"
        let! v1 = Box ("b", ())
        return state + v1
    }
    printfn "%A" a

    let b = scope {
        let! state = initLater
        let! v1 = Box ("b", ())
        return state + v1
    }
    printfn "%A" b
