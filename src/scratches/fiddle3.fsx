
#r "../LocSta/bin/Debug/netstandard2.0/LocSta.dll"
open LocSta

type Gen.LoopBuilder with
    member _.For(sequence: list<'a>, body) =
        let results = sequence |> List.map body
        Gen.ofGenResultsRepeating
