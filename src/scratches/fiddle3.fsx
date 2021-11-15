
#r "../FsLocalState/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

type Gen.LoopBuilder with
    member _.For(sequence: list<'a>, body) =
        let results = sequence |> List.map body
        Gen.ofGenResultsRepeating
