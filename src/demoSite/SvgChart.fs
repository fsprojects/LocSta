module DemoSite.SvgChart

open DemoSite.Types

let private f (v: float) = sprintf "%.1f" v

let renderChart (allSeries: ChartSeries list) =
    let width = 600
    let height = 200
    let margin = 40
    let marginRight = 10
    let marginTop = 10
    let marginBottom = 25
    let plotW = float (width - margin - marginRight)
    let plotH = float (height - marginTop - marginBottom)

    let allValues = allSeries |> List.collect (fun s -> s.Values)
    let maxLen = allSeries |> List.map (fun s -> s.Values.Length) |> List.max

    let yMin, yMax =
        let mn = allValues |> List.min
        let mx = allValues |> List.max
        if mn = mx then mn - 1.0, mx + 1.0
        else
            let pad = (mx - mn) * 0.08
            mn - pad, mx + pad
    let yRange = yMax - yMin

    let mapX i =
        float margin + (if maxLen <= 1 then 0.0 else float i / float (maxLen - 1) * plotW)
    let mapY v =
        float marginTop + (1.0 - (v - yMin) / yRange) * plotH

    let gridLines =
        [ for g in 0..4 ->
            let gv = yMin + yRange * float g / 4.0
            let gy = mapY gv
            sprintf """<line x1="%s" y1="%s" x2="%s" y2="%s" stroke="#eee" stroke-width="1"/>
<text x="%s" y="%s" text-anchor="end" font-size="9" fill="#999">%.3g</text>"""
                (f (float margin)) (f gy) (f (float (width - marginRight))) (f gy)
                (f (float margin - 4.0)) (f (gy + 3.0)) gv ]
        |> String.concat "\n"

    let zeroLine =
        if yMin < 0.0 && yMax > 0.0 then
            let zy = mapY 0.0
            sprintf """<line x1="%s" y1="%s" x2="%s" y2="%s" stroke="#ccc" stroke-width="1" stroke-dasharray="4,3"/>"""
                (f (float margin)) (f zy) (f (float (width - marginRight))) (f zy)
        else ""

    let dataSeries =
        [ for s in allSeries do
            if s.Values.Length > 0 then
                if s.IsStep then
                    let pathData =
                        let start = sprintf "M%s,%s" (f (mapX 0)) (f (mapY s.Values.[0]))
                        let steps =
                            s.Values |> List.indexed |> List.tail
                            |> List.map (fun (i, v) ->
                                sprintf "H%s V%s" (f (mapX i)) (f (mapY v)))
                        start :: steps |> String.concat " "
                    yield sprintf """<path d="%s" fill="none" stroke="%s" stroke-width="1.5" opacity="0.85"/>"""
                        pathData s.Color
                else
                    let points =
                        s.Values |> List.mapi (fun i v ->
                            sprintf "%s,%s" (f (mapX i)) (f (mapY v)))
                        |> String.concat " "
                    yield sprintf """<polyline points="%s" fill="none" stroke="%s" stroke-width="1.5" opacity="0.85"/>"""
                        points s.Color ]
        |> String.concat "\n"

    let xTicks =
        if maxLen > 0 then
            let tickCount = min maxLen 6
            [ for t in 0..tickCount-1 ->
                let idx = if maxLen <= 1 then 0 else int (float t / float (tickCount - 1) * float (maxLen - 1))
                sprintf """<text x="%s" y="%s" text-anchor="middle" font-size="9" fill="#999">%d</text>"""
                    (f (mapX idx)) (f (float height - 4.0)) idx ]
            |> String.concat "\n"
        else ""

    let legend =
        if allSeries.Length > 1 then
            [ for i in 0..allSeries.Length-1 ->
                let s = allSeries.[i]
                let ly = 8 + i * 14
                let lx = margin + 6
                sprintf """<rect x="%d" y="%d" width="12" height="3" fill="%s" rx="1"/>
<text x="%s" y="%s" text-anchor="start" font-size="9" fill="#666">%s</text>"""
                    lx ly s.Color (f (float lx + 16.0)) (f (float ly + 4.0)) s.Label ]
            |> String.concat "\n"
        else ""

    sprintf """<svg width="%d" height="%d" viewBox="0 0 %d %d" xmlns="http://www.w3.org/2000/svg" style="background:#fff;border:1px solid #e0e0e0;border-radius:6px;display:block">
%s
%s
%s
%s
%s
</svg>""" width height width height gridLines zeroLine dataSeries xTicks legend
