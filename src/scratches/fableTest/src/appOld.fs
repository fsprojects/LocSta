module App

open LocSta
open LocSta.Gen

open Browser
open Browser.Types

type Sender = Id of int

// TODO: Use reader instead of this hack
[<AllowNullLiteral>]
type App(appElement: HTMLElement, triggerUpdate: App -> HTMLElement) =
    let mutable currId = -1
    member val CurrentSender: Sender option = None with get, set
    member _.NewSender() =
        currId <- currId + 1
        printfn $"New sender: {currId}"
        Id currId
    member _.CreateElement name =
        printfn $"Create: {name}"
        document.createElement name
    member this.Run() =
        let initialElement = triggerUpdate this
        appElement.appendChild initialElement |> ignore
    member this.TriggerUpdate sender =
        this.CurrentSender <- sender
        printfn $"Trigger update with sender: {sender}"
        let element = triggerUpdate this
        ()

let app : Gen<App,_,_> = fun s r -> r,()

type NodeList with
    member this.Seq = seq { for i in 0 .. this.length-1 do this.Item i }

let genList (children: Gen<_,_,_> list) =
    fun (s: CombinedBoxedState list) r ->
        let results =
            [ for g in children do
                g None r
            ]



let elem name attributes child =
    loop {
        let! app = app
        let! elem = preserve (fun () -> app.CreateElement name)
        printfn $"Eval: {name} ({elem.GetHashCode()})"
        let! child = child |> Gen.map (fun x -> x :> Node)
        do for aname,avalue in attributes do
            let elemAttr = elem.attributes.getNamedItem aname
            if elemAttr.value <> avalue then
                elemAttr.value <- avalue
        do if elem.childNodes.Seq |> Seq.contains child |> not then
            printfn $"add child (node count = {elem.childNodes.length})"
            elem.appendChild child |> ignore
        return elem
    }

let text content = 
    loop {
        let! elem = preserve (fun () -> document.createTextNode content)
        do if elem.textContent <> content then
            elem.textContent <- content
        return elem
    }
let div attributes content = elem "div" attributes content
let p attributes content = elem "p" attributes content
let button content click = loop {
    let! app = app
    let! button = elem "button" [] content |> Gen.map (fun x -> x :?> HTMLButtonElement)
    button.onclick <- fun _ ->
        printfn "-----CLICK"
        click ()
        app.TriggerUpdate None
    return button
}

let inline ( ~% ) g = g |> Gen.map (fun x -> x :> HTMLElement)

let view() = loop {
    let comp = loop {
        let! count, setCount = Gen.ofMutable 0
        return!
            div [] (
                button (text $"Count = {count}") (fun () -> setCount (count + 1))
            )
    }

    let x = [
        % comp
        % button (comp) id
    ]

    let! c1 = comp
    let! c2 = comp
    let! wrapper = div [] (preserve (fun () -> document.createTextNode "---"))
    do if wrapper.childNodes.length = 1 then
        wrapper.appendChild c1 |> ignore
        wrapper.appendChild c2 |> ignore
    return wrapper
}


do
    App(
        document.querySelector("#app") :?> HTMLDivElement,
        view() |> Gen.toEvaluable
    ).Run()
