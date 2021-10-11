module FeedbackTests

open System
open FsLocalState
open Xunit


/// The type of the reader state fot the tests - here, unit.
type Env = unit

 /// An 1-incremental counter with min (seed) and max, written in "feedback" notation.
 /// When max is reached, counting begins with min again.
let counterGen exclMin inclMax =
    exclMin => fun state -> gen {
        let newValue = 1 + (if state = inclMax then exclMin else state)
        return newValue, newValue
    }

let network seed counterMin counterMax =
    seed => fun state ->
         gen {
             let! i = counterGen counterMin counterMax
             let newValue = state + i
             return newValue, newValue
         }

let counterMin = 0
let counterMax = 20

let accuSeed = 0

let sampleCount = 1000

module Counter =

    ()
    let counted =
        let x =
            gen {
                let! i = counterGen counterMin counterMax
                return i } 
        x |> Gen.toListn sampleCount

    [<Fact>]
    let ``Sample count`` () =
        Assert.Equal(counted.Length, sampleCount)

    [<Fact>]
    let ``Min is exclusive`` () =
        Assert.Equal(counted |> List.min, counterMin + 1)

    [<Fact>]
    let ``Max is inclusive`` () =
        Assert.Equal(counted |> List.max, counterMax)

    [<Fact>]
    let ``Incremental and reset`` () =

        let lastAndCurrent (l: 'a list) =
            l.Tail
            |> Seq.zip l
            |> Seq.toList

        counted
        |> lastAndCurrent
        |> List.map (fun (last, current) -> current = last + 1 || current = counterMin + 1 && last = counterMax)
        |> List.forall (fun x -> x = true)
        |> Assert.True
