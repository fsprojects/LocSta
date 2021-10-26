
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
