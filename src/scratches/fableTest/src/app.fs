module App

open LocSta

open Browser.Dom

//// Mutable variable to count the number of times we clicked the button
//let mutable count = 0

//// Get a reference to our button and cast the Element to an HTMLButtonElement
//let myButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement

//// Register our listener
//myButton.onclick <- fun _ ->
//    count <- count + 1
//    myButton.innerText <- sprintf "XXX YYY You clicked: %i time(s)" count

let app = document.querySelector("#app") :?> Browser.Types.HTMLDivElement

let view =
    feed {
        let! node = InitWith (fun () ->
            let p = document.createElement("p")
            do app.appendChild(p) |> ignore
            p
        )
        let! count = count 0 1
        do node.textContent <- string count
        return Feed.Emit (count, node)
    }
    |> Gen.toEvaluable

let evaluateButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement
evaluateButton.onclick <- fun _ ->
    let currentCount = view.GetNext()
    console.log($"Count = {currentCount}")
    



//let app = document.querySelector("#app") :?> Browser.Types.HTMLDivElement
//let node = app.appendChild(document.createElement("p"))
//node.textContent <- "sdfsdxxxxxf"
