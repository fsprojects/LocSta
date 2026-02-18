module DemoSite.App

open Browser.Dom
open DemoSite.Render

let private app = document.getElementById "app"
app.innerHTML <- renderShell ()

let private content = document.getElementById "content"

let private render () =
    content.innerHTML <- renderContent window.location.hash

render ()

window.addEventListener ("hashchange", fun _ -> render ())
