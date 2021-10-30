
#r "FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

let delay input seed =
    seed => fun state -> gen {
        return Value ((state, input), ())
    }

let get seed =
    fun (state: Gen.FeedbackState<_,_>) ->
        Value ((), ())
    |> Gen.create
    
let delay input seed =
    gen {
        let! state = get seed
        return Value ((state, input), ())
    }
    
let feedback seed =
    gen {

    }

gen {
    let! val1 = feedback 12 {
        let! state = get
    }
}