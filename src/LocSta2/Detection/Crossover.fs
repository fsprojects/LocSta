module LocSta.Blocks.Detection.Crossover

open LocSta.Core

/// Detects when stream s1 crosses above (1) or below (-1) stream s2; 0 otherwise.
let crossover s1 s2 =
    stream {
        let! v1 = s1
        let! v2 = s2
        let! st = useMemoWith (fun () -> MutableValue(0.0, false))
        let (prevDiff, hasPrev) = st.Value
        let currDiff = v1 - v2
        let output =
            if not hasPrev then 0
            elif prevDiff <= 0.0 && currDiff > 0.0 then 1
            elif prevDiff >= 0.0 && currDiff < 0.0 then -1
            else 0
        st.Value <- (currDiff, true)
        return output
    }
