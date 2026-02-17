module LocSta.Blocks.Windowing.Segment

open LocSta.Core

/// Accumulates values into segments delimited by a boundary flag. Input: (value, boundary).
let segment () =
    stream {
        let! (value, boundary) = getCtx()
        let! buffer = useState []
        buffer.Value <- value :: buffer.Value
        if boundary then
            let result = buffer.Value |> List.rev
            buffer.Value <- []
            return Some result
        else
            return None
    }
