module App

open LocSta
open Browser
open Browser.Types

type ElementId = Id of int

// TODO: Use reader instead of this hack
[<AllowNullLiteral>]
type App(appElement: HTMLElement, triggerUpdate: ElementId option -> HTMLElement) =
    let mutable id = -1
    let mutable currentTrigger = None
    member _.createElement name =
        id <- id + 1
        document.createElement name, Id id
    member _.run() =
        let element = triggerUpdate None
        do appElement.appendChild element |> ignore
    member _.triggerUpdate id =
        let element = triggerUpdate (Some id)
        // TODO: element <> appElement.child -> throw
        ()

let mutable app: App = null

let toSeq (coll: NodeList) = seq { for i in 0..coll.length-1 do coll.Item i }

let elem name attributes child =
    fun state ->
        let elem,id = state |> Option.defaultWith (fun () -> app.createElement name)
        for aname,avalue in attributes do
            let elemAttr = elem.attributes.getNamedItem aname
            if elemAttr.value <> avalue then
                elemAttr.value <- avalue
        if toSeq elem.childNodes |> Seq.contains child |> not then
            elem.appendChild child |> ignore
        Res.Loop.emit elem (elem,id)
    |> Gen.createLoop

let text content = document.createTextNode content
let div attributes content = elem "div" attributes content
let p attributes content = elem "p" attributes content
let button (content: string) click = loop {
    let! button = elem "button" [] (text content)
    let button = button :?> HTMLButtonElement
    match click with
    | Some click ->
        printfn "register click"
        button.onclick <- click
    | None -> 
        printfn "no click"
        button.onclick <- fun _ -> ()
    button
}

let view = 
    loop {
        let! c = Gen.count 0 1
        let! button = button "Increment" (Some (fun args -> printfn "Clicked"))
        return! div [] button
    }
    |> Gen.toEvaluable



app <- App(
    document.querySelector("#app") :?> HTMLDivElement,
    fun _ -> view.Evaluate().Value
)

// let evaluateButton = document.querySelector(".my-button") :?> HTMLButtonElement
// evaluateButton.onclick <- fun _ -> view.Evaluate()

app.run()
