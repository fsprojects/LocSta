
module IntegrationTests

open FsUnit
open FsLocalState
open NUnit.Framework


/// An 1-incremental counter with min (seed) and max, written in "feedback" notation.
/// When max is reached, counting begins with min again.
let counter exclMin inclMax =
    fdb {
        let! state = init exclMin
        let newValue = (if state = inclMax then exclMin else state) + 1
        return Res.feedback newValue newValue
    }

/// An accumulator function summing up incoming values, starting with the given seed.
let accu value seed =
    fdb {
        let! state = init seed
        let newValue = state + value
        return Res.feedback newValue newValue
    }


let counterMin = 0
let counterMax = 20
let accuSeed = 0
let sampleCount = 1000

let counted =
    gen {
        let! i = counter counterMin counterMax
        return Res.value i
    }
    |> Gen.toListn sampleCount

let [<TestCase>] ``Sample count`` () =
    counted.Length |> should equal sampleCount

let [<TestCase>] ``Min is exclusive`` () =
    counterMin + 1 |> should equal (counted |> List.min)

let [<TestCase>] ``Max is inclusive`` () =
    counterMax |> should equal (counted |> List.max)

let [<TestCase>] ``Incremental and reset`` () =
    let lastAndCurrent (l: 'a list) =
        l.Tail
        |> Seq.zip l
        |> Seq.toList
    counted
    |> lastAndCurrent
    |> List.map (fun (last, current) -> current = last + 1 || current = counterMin + 1 && last = counterMax)
    |> List.forall (fun x -> x = true)
    |> should equal true

module CounterAndAccu =

    let counterMin = 0
    let counterMax = 20
    let accuSeed = 0
    let sampleCount = 1000

    let accumulated =
        gen {
            let! i = counter counterMin counterMax
            let! acc = accu i accuSeed
            return Res.value acc
        }
        |> Gen.toListn sampleCount

    let [<TestCase>] ``Sample count`` () =
        sampleCount |> should equal accumulated.Length

    let [<TestCase>] ``Gradient between counter min/max`` () =

        let lastAndCurrent (l: 'a list) =
            l.Tail
            |> Seq.zip l
            |> Seq.toList

        accumulated
        |> lastAndCurrent
        |> List.map (fun (last, current) -> current >= last + counterMin + 1 && current <= last + counterMax)
        |> List.forall (fun x -> x = true)
        |> Assert.True
    