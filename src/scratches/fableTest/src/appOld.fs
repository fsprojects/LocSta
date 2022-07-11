// TODOs:
//  - Don't calc the whole tree when triggering Update
//  - first class task/async support (in gen)
//  - implement "for" in ChildBuilder

module App

open System

open LocSta
open LocSta.Gen

open Browser
open Browser.Types

[<AutoOpen>]
module Application =
    type Sender = Id of int
    
    // TODO: Braucht man den Sender-Kram noch?
    [<AllowNullLiteral>]
    type App(document: Document, appElement: Element, triggerUpdate: App -> Node) =
        let mutable currId = -1
        member val CurrentSender: Sender option = None with get, set
        member _.NewSender() =
            currId <- currId + 1
            printfn $"New sender: {currId}"
            Id currId
        member _.Document = document
        member this.Run() =
            let initialElement = triggerUpdate this
            appElement.appendChild initialElement |> ignore
        member this.TriggerUpdate sender =
            this.CurrentSender <- sender
            printfn $"Trigger update with sender: {sender}"
            let element = triggerUpdate this
            ()

    let app : Gen<App,_,_> = fun s r -> r,()

[<AutoOpen>]
module HelperAndExtensions =
    type NodeList with
        member this.elements = seq { for i in 0 .. this.length-1 do this.Item i }
    type Node with
        member this.clearChildren() = this.textContent <- "" // TODO: really?

[<AutoOpen>]
module Framework =
    type HtmlGen<'s> = Gen<Node, 's, App>
    type ChildGen = Type * HtmlGen<obj>

    let inline boxGen (stateType: Type) (g: HtmlGen<'s>) : ChildGen =
        let g: HtmlGen<obj> = fun s r ->
            let o,s = g (unbox s) r
            o, box s
        stateType, g

    // TODO: Add overloads for yield (string, int, etc.)
    type ChildrenBuilder<'s>(run: ChildGen list -> HtmlGen<'s>) =
        member inline _.Yield<'s1>(x : HtmlGen<'s1>) = [boxGen typeof<'s1> x]
        member inline _.Delay([<InlineIfLambda>] f) = f ()
        member _.Combine(a, b) = List.append a b
        member _.Zero() = []
        member _.Run(children) = run children

    let elem name attributes (children: ChildGen list) =
        console.log(children)
        let syncChildren (elem: Node) : Gen<_,_,_> = fun s r ->
            let s = s |> Option.defaultWith (fun () -> ResizeArray())
            let newState =
                seq {
                    for childType,childGen in children do
                        let stateIdx = s |> Seq.tryFindIndex (fun (typ,_) -> typ = childType)
                        let newChildState =
                            match stateIdx with
                            | Some idx ->
                                let childState = s[idx]
                                do s.RemoveAt(idx)
                                childGen (childState |> snd |> Some) r |> snd
                            | None ->
                                let o,s = childGen None r
                                do elem.appendChild o |> ignore
                                s
                        yield childType,newChildState
                }
            (), ResizeArray newState
        loop {
            let! app = app
            let! elem = preserve (fun () -> app.Document.createElement name :> Node)
            printfn $"Eval: {name} ({elem.GetHashCode()})"
            // sync attrs
            do for aname,avalue in attributes do
                let elemAttr = elem.attributes.getNamedItem aname
                if elemAttr.value <> avalue then
                    elemAttr.value <- avalue
            do! syncChildren elem
            return elem
        }

[<AutoOpen>]
module HtmlElementsApi =
    let text text =
        loop {
            let! app = app
            let! elem = preserve (fun () -> app.Document.createTextNode text)
            do if elem.textContent <> text then
                elem.textContent <- text
            return elem :> Node
        }

    let div attributes = ChildrenBuilder(elem "div" attributes)

    let p attributes = ChildrenBuilder(elem "p" attributes)

    let button attributes click =
        ChildrenBuilder(fun children -> loop {
            let! app = app
            // TODO: Optimize the map afterwards; that's not necessary
            let! button =
                elem "button" attributes children
                |> Gen.map (fun x -> x :?> HTMLButtonElement)
            button.onclick <- fun _ ->
                printfn "-----CLICK"
                click ()
                app.TriggerUpdate None
            return button
        })




let comp = 
    loop {
        let! count, setCount = Gen.ofMutable 0
        return!
            div [] {
                button [] (fun () -> setCount (count + 1)) {
                    text $"Count = {count}"
                }
            }
    }


let view() = 
    div [] {
        comp
        div [] {
            text "Hurz"
            comp
        }
    }

do
    App(
        document,
        document.querySelector("#app"),
        view() |> Gen.toEvaluable
    ).Run()
