module LocSta.Blocks.Generators.Fibonacci

open LocSta.Core

/// Generates the Fibonacci sequence (1, 2, 3, 5, 8, 13, ...).
let fibonacci () =
    stream {
        let! st = useMemoWith (fun () -> MutableValue(0, 1))
        let (a, b) = st.Value
        let current = a + b
        st.Value <- (b, current)
        return current
    }
