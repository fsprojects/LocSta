module LocSta.Blocks

let counter start increment =
    stream {
        let! state = useState start
        let current = state.Value
        state.Value <- current + increment
        return current
    }

let fibonacci () =
    stream {
        let! prev1 = useState 0
        let! prev2 = useState 1
        let current = prev1.Value + prev2.Value
        prev1.Value <- prev2.Value
        prev2.Value <- current
        return current
    }

let movingAverage windowSize =
    stream {
        let! ctx = getCtx()
        let! window = useState []
        let newWindow = (ctx :: window.Value) |> List.truncate windowSize
        window.Value <- newWindow
        return (newWindow |> List.sum |> float) / (float newWindow.Length)
    }

let delayByN n defaultValue =
    stream {
        let! ctx = getCtx()
        let! buffer = useStateWith (fun () -> System.Collections.Generic.Queue<_>(List.replicate n defaultValue))
        let output =
            if buffer.Value.Count = 0 then ctx
            else buffer.Value.Dequeue()
        if n > 0 then buffer.Value.Enqueue ctx
        return output
    }

let delayBy1 defaultValue = delayByN 1 defaultValue

let inline binOp ([<InlineIfLambda>] op) s1 s2 =
    stream {
        let! v1 = s1
        let! v2 = s2
        return op v1 v2
    }

let inline ( .+. ) s1 s2 = binOp (+) s1 s2
let inline ( .-. ) s1 s2 = binOp (-) s1 s2
let inline ( .*. ) s1 s2 = binOp (*) s1 s2
let inline ( ./. ) s1 s2 = binOp (/) s1 s2

