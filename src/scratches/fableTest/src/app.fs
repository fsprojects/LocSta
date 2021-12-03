module App

open LocSta

open Browser.Dom

let app = document.querySelector("#app") :?> Browser.Types.HTMLDivElement

let createElem name = document.createElement name |> app.appendChild

let elem name =
    fun state ->
        let element = state |> Option.defaultWith (fun () -> createElem name)
        Res.Loop.emit element element
    |> Gen.createLoop

let view =
    loop {
        let! p = elem "p"
        let! count = count 0 1
        do p.textContent <- (string count)
        return Loop.Emit p
    }
    |> Gen.toEvaluable

let evaluateButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement
evaluateButton.onclick <- fun _ -> 
    view.Evaluate()
