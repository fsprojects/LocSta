module BaseTests

open System
open FsLocalState
open FsLocalState.Tests
open Xunit


/// The type of the reader state fot the tests - here, unit.
type Env = unit

/// An 1-incremental counter with min (seed) and max, written in "feedback" notation.
/// When max is reached, counting begins with min again.
let counterGen exclMin inclMax =
    exclMin <|> fun state (env: Env) ->
        local {
            let newValue =
                (if state = inclMax then exclMin else state)
                + 1
            return { value = newValue
                     state = newValue }
        }

/// An accumulator function summing up incoming values, starting with the given seed.
let accuFx seed value =
    seed <|> fun state (env: Env) ->
        local {
            let newValue = state + value
            return { value = newValue
                     state = newValue }
        }

let counterMin = 0
let counterMax = 20

let accuSeed = 0

let sampleCount = 1000

module Counter =

    let counted =
        local {
            let! i = counterGen counterMin counterMax
            return i
        }
        |> TestHelper.toValuesN sampleCount

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

module CounterAndAccu =

    let accumulated =
        local {
            let! i = counterGen counterMin counterMax
            let! acc = accuFx accuSeed i
            return acc
        }
        |> TestHelper.toValuesN sampleCount

    [<Fact>]
    let ``Sample count`` () =
        Assert.Equal(accumulated.Length, sampleCount)

    [<Fact>]
    let ``Gradient between counter min/max`` () =

        let lastAndCurrent (l: 'a list) =
            l.Tail
            |> Seq.zip l
            |> Seq.toList

        accumulated
        |> lastAndCurrent
        |> List.map (fun (last, current) -> current >= last + counterMin + 1 && current <= last + counterMax)
        |> List.forall (fun x -> x = true)
        |> Assert.True
