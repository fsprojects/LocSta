module LocSta.Blocks.Generators.Counter

open LocSta.Core

/// Generates an incrementing counter starting at 'start' with given 'increment'.
let counter start increment =
    stream {
        let! state = useState start
        let current = state.Value
        state.Value <- current + increment
        return current
    }
