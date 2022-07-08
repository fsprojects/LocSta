module App

open LocSta
open Browser
open Browser.Types

type Sender = Id of int

// TODO: Use reader instead of this hack
[<AllowNullLiteral>]
type App(appElement: HTMLElement, triggerUpdate: Sender option -> HTMLElement) =
    let mutable lastSender = None
    let mutable currId = -1
    member _.CurrentSender: Sender option = lastSender
    member _.NewSender() =
        currId <- currId + 1
        printfn $"New sender: {currId}"
        Id currId
    member _.CreateElement name =
        printfn $"Create: {name}"
        document.createElement name
    member _.Run() =
        let element = triggerUpdate None
        do appElement.appendChild element |> ignore
    member _.TriggerUpdate sender =
        lastSender <- sender
        printfn $"Trigger update with sender: {sender}"
        let element = triggerUpdate sender
        // TODO: element <> appElement.child -> throw
        ()

let mutable app: App = null

let toSeq (coll: NodeList) = seq { for i in 0..coll.length-1 do coll.Item i }

let elem name attributes child =
    feed {
        let! elem = InitWith (fun () -> app.CreateElement name)
        let! child = child |> Gen.map (fun x -> x :> Node)
        do for aname,avalue in attributes do
            let elemAttr = elem.attributes.getNamedItem aname
            if elemAttr.value <> avalue then
                elemAttr.value <- avalue
        if toSeq elem.childNodes |> Seq.contains child |> not then
            printfn $"add child (node count = {elem.childNodes.length})"
            elem.appendChild child |> ignore
        return Feed.Emit (elem, elem)
    }

let text content = feed {
    let! elem = InitWith (fun () -> document.createTextNode content)
    do if elem.textContent <> content then
        elem.textContent <- content
    return Feed.Emit (elem, elem)
}
let div attributes content = elem "div" attributes content
let p attributes content = elem "p" attributes content
let button content click = loop {
    let! clickId = Gen.initWith app.NewSender
    do if app.CurrentSender = Some clickId then
        click()
    let! button = elem "button" [] content |> Gen.map (fun x -> x :?> HTMLButtonElement)
    button.onclick <- fun _ -> app.TriggerUpdate (Some clickId)
    button
}

let view() = loop {
    let comp() = feed {
        let! c = InitWith (fun () -> 0)
        let mutable c = c
        let onClick = (fun () -> 
            c <- c + 1
            printfn $"count = {c}")
        let! div = div [] (
            button (text $"Count = {c}") onClick
        )
        return Feed.Emit (div, c)
    }

    let! c1 = comp()
    let! c2 = comp()
    let! wrapper = div [] (Gen.initWith (fun () -> document.createTextNode "---"))
    do if wrapper.childNodes.length = 1 then
        wrapper.appendChild c1 |> ignore
        wrapper.appendChild c2 |> ignore
    wrapper
}


do
    let evaluableView = view() |> Gen.toEvaluable
    app <- App(
        document.querySelector("#app") :?> HTMLDivElement,
        fun _ -> evaluableView.Evaluate().Value
    )

    app.Run()
