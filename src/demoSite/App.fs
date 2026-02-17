module DemoSite.App

open Browser.Dom
open DemoSite.Render

let private app = document.getElementById "app"

let private render () =
    app.innerHTML <- renderApp window.location.hash

render ()

window.addEventListener ("hashchange", fun _ -> render ())
