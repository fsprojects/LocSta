module LocSta.Blocks.Operators

open LocSta.Core

/// Combines two streams with a binary operator.
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
