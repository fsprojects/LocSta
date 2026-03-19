module DemoSite.App

open Browser.Dom
open DemoSite.Render

let private app = document.getElementById "app"
app.innerHTML <- renderShell ()

let private content = document.getElementById "content"
let private sidebar = document.getElementById "sidebar"
let private overlay = document.getElementById "sidebar-overlay"
let private hamburger = document.getElementById "hamburger"

let private closeSidebar () =
    sidebar.classList.remove "open"
    overlay.classList.remove "open"

let private toggleSidebar () =
    sidebar.classList.toggle "open" |> ignore
    overlay.classList.toggle "open" |> ignore

hamburger.addEventListener ("click", fun _ -> toggleSidebar ())
overlay.addEventListener ("click", fun _ -> closeSidebar ())

let private render () =
    content.innerHTML <- renderContent window.location.hash
    closeSidebar ()

render ()

window.addEventListener ("hashchange", fun _ -> render ())
