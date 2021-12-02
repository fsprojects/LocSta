
#if INTERACTIVE
#load "../../../LocSta/core.fs"
#load "../../../LocSta/lib.fs"
#endif

open LocSta
open LocSta.Lib.Gen







let html elemName = [
    $"<{elemName}>"

    $"</{elemName}>"
]

let view =
    feed {
        let! state = Init 0
        let! _ = count 0 1

        yield 1, state + 1
    }
    |> Gen.toListn 10


//let view =
//    let (count, setCount) = React.useState(0)
//    Html.div {
//        Html.button {
//            prop.style [ style.marginRight 5 ]
//            prop.onClick (fun _ -> setCount(count + 1))
//            prop.text "Increment"
//        }

//        Html.button {
//            prop.style [ style.marginLeft 5 ]
//            prop.onClick (fun _ -> setCount(count - 1))
//            prop.text "Decrement"
//        }

//        Html.h1 count
//    }
