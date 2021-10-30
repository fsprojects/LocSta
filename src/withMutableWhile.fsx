
type GenRes<'v> =
    | Value of 'v
    | Discard
    | Stop

type Gen<'v> =
    { guard: unit -> bool
      body: unit -> GenRes<'v> }

type GenBuilder() =
    member this.Bind(m: Gen<'a>, f: 'a -> Gen<'b>) =
        if m.guard() then
            let mres = m.body()
            match mres with
            | Value mv -> 
                let fres = f mv
                match fres with
                | Value fv ->
                    { guard = m.guard
                      body = fun () -> Discard }
            | Discard ->
                { guard = m.guard
                  body = fun () -> Discard }
            | Stop ->
                { guard = fun () -> false
                  body = fun () -> Stop }
        else
            { guard = fun () -> false
              body = fun () -> Stop }
    member this.Yield(x) = x
    member this.Return(x) =
        { guard = fun () -> true
          body = fun () -> Value x }
    member this.Delay(f) = f
    member this.Run(f) = f ()
    member this.While(guard, body) =
        { guard = guard
          body = body }
        //if not (guard ()) then
        //    Stop
        //else
        //    let res = body ()
        //    Cont (res, guard, body)

let rec eval (g: Gen<_>) =
    let mutable run = true
    seq {
        while g.guard() do
            let v = g.body()
            match v with
            | Value res ->
                yield res
            | Stop ->
                run <- false
            | Discard ->
                ()
    }

let gen = GenBuilder()

let count start inc max =
    gen {
        let mutable curr = start
        while curr < max do
            curr <- curr + inc
            yield Value curr
    }

let test = gen {
    let! c1 = count 0 1 10
    let! c2 = count 100 5 200
    return c1 + c2
}

let test =
    let c1 = count 0 1 10
    match c1 with
    | 
    let! c2 = count 100 5 200
    return c1 + c2

count 0 1 10 |> eval

test |> eval
