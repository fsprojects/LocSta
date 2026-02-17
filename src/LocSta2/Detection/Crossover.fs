module LocSta.Blocks.Detection.Crossover

open LocSta.Core

/// Detects when stream s1 crosses above (1) or below (-1) stream s2; 0 otherwise.
let crossover s1 s2 =
    stream {
        let! v1 = s1
        let! v2 = s2
        let! prevDiff = useState ValueNone
        let currDiff = v1 - v2
        let output =
            match prevDiff.Value with
            | ValueNone -> 0
            | ValueSome pd ->
                if pd <= 0.0 && currDiff > 0.0 then 1
                elif pd >= 0.0 && currDiff < 0.0 then -1
                else 0
        prevDiff.Value <- ValueSome currDiff
        return output
    }
