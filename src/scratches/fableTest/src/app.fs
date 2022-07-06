module App

open LocSta
open Browser
open Browser.Types

type HtmlElementEx = seq<string * string> -> string -> LoopGen<HTMLElement, HTMLElement>

let createElem name = document.createElement name

let elem name attributes content =
    fun state ->
        let element = state |> Option.defaultWith (fun () -> document.createElement name)
        for aname,avalue in attributes do
            let elemAttr = element.attributes.getNamedItem aname
            if elemAttr.value <> avalue then
                elemAttr.value <- avalue
        if element.innerHTML <> content then
            element.innerHTML <- content
        Res.Loop.emit element element
    |> Gen.createLoop

let div: HtmlElementEx = elem "div"
let button: HtmlElementEx = elem "button"
let p: HtmlElementEx = elem "p"

let view = 
    loop {
        let! c = Gen.count 0 1
        return! div [] $"Count = {c}"
    }
    |> Gen.toEvaluable



let app = document.querySelector("#app") :?> HTMLDivElement
let initial = view.Evaluate().Value
do app.appendChild initial |> ignore

let evaluateButton = document.querySelector(".my-button") :?> HTMLButtonElement
evaluateButton.onclick <- fun _ -> view.Evaluate()
