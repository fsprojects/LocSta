
#r "nuget: XPlot.Plotly"
#r "../../src/FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"

open System
open FsLocalState
open XPlot.Plotly


let monteCarlo =
    0 => fun lastInsideCount -> gen {
        let! samples = Gen.count 1 1
        let! x = Gen.random ()
        let! y = Gen.random ()
        let distance = Math.Sqrt (x*x + y*y)
        let isInsideCircle = distance < 1.0
        // let! insideCount = isInsideCircle <?> count <!> lastInsideCount
        let insideCount = lastInsideCount + (if isInsideCircle then 1 else 0)
        let pi = 4.0 * float insideCount / float samples
        return pi, insideCount
    }


// evaluate and plot pi

let numSamples = 2_000

let calculatedPi = monteCarlo |> Gen.toList numSamples
let constPi = List.init numSamples (fun _ -> Math.PI)

[
    calculatedPi
    constPi
]
|> List.map (fun s -> Scatter (y = s))
|> Chart.Plot
|> Chart.WithId "Hurz"
|> Chart.Show
