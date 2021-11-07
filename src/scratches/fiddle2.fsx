
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

gen {
    let! c = count 0 1
    return Control.Emit c
}
|> Gen.toListn 10

fdb {
    let! state = Init "Hello"
    let! c = count 0 1
    return Control.Feedback ($"{state} {c}", "")
}
|> Gen.toListn 10


let x =
    Init 0 |> Gen.bindFdb (fun state ->
        Gen.returnFeedback state 12
    )

let y =
    Init 0 |> Gen.bindFdb (fun state ->
        count 0 1 |> Gen.bind (fun c ->
            Gen.returnFeedback state (c + 1)
        )
    )
