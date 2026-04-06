module LocSta.Blocks.Windowing.Segment

open LocSta.Core

/// Accumulates values into segments delimited by a boundary flag. Input: (value, boundary).
let segment () =
    stream {
        let! (value, boundary) = getCtx()
        let! buffer = useStateWith (fun () -> ResizeArray<_>())
        buffer.Value.Add(value)
        if boundary then
            let result = Seq.toList buffer.Value
            buffer.Value.Clear()
            return Some result
        else
            return None
    }
