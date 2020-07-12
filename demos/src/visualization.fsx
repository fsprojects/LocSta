
#r "../packages/XPlot.Plotly/lib/netstandard2.0/XPlot.Plotly.dll"
#r "../lib/FsLocalState.dll"

open System
open FsLocalState
open FsLocalState.Operators
open XPlot.Plotly


let monteCarlo =
    0 <|> fun lastInsideCount (_: unit) -> gen {
        let! samples = Gen.countFrom 1 1
        let! x = Gen.random()
        let! y = Gen.random()
        let distance = Math.Sqrt (x*x + y*y)
        let isInsideCircle = distance < 1.0
        // let! insideCount = isInsideCircle <?> count <!> lastInsideCount
        let insideCount = if isInsideCircle then lastInsideCount + 1 else lastInsideCount
        let pi = 4.0 * float insideCount / float samples
        return { value = pi; state = insideCount }
    }


// evaluate and plot pi

let numSamples = 2_000

let piSeq = monteCarlo |> Gen.toSeq ignore
let calculatedPi = piSeq |> Seq.toListN numSamples

let constPi = List.init numSamples (fun _ -> Math.PI)

[calculatedPi; constPi]
|> List.map (fun s -> Scatter(y = s))
|> Chart.Plot
|> Chart.WithId "Hurz"
|> Chart.Show
