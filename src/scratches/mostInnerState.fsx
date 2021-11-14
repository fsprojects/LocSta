
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

//let read seed =
//    fun state ->
//        printfn "READ State: %A" state
//        let res = state |> Option.defaultValue seed
//        Value (res, res)
//    |> Gen.create

let test = gen {
    //let! feedback = read "A"
    let! c11 = count_0_1
    let! c12 = count_0_1
    let! c13 = gen {
        let! c21 = count_0_1
        let! c22 = count_0_1
        return Value((c21 + c22, "World"), ())
    }
    return Value(c11 + c12, "Hello")
}

test |> Gen.toSeqState |> Seq.truncate 5 |> Seq.toList

