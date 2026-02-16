module LocSta.Blocks.Generators.Fibonacci

open LocSta.Core

/// Generates the Fibonacci sequence (1, 2, 3, 5, 8, 13, ...).
let fibonacci () =
    stream {
        let! prev1 = useState 0
        let! prev2 = useState 1
        let current = prev1.Value + prev2.Value
        prev1.Value <- prev2.Value
        prev2.Value <- current
        return current
    }
