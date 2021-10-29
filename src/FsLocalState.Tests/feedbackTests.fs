module FeedbackTests

open System
open FsUnit
open FsLocalState
open NUnit.Framework

 /// An 1-incremental counter with min (seed) and max, written in "feedback" notation.
 /// When max is reached, counting begins with min again.
let counter exclMin inclMax =
    exclMin => fun state -> gen {
        let newValue = 1 + (if state = inclMax then exclMin else state)
        return Value ((newValue, newValue), ())
    }

let network seed counterMin counterMax =
    seed => fun state ->
         gen {
             let! i = counter counterMin counterMax
             let newValue = state + i
             return Value ((newValue, newValue), ())
         }

let counterMin = 0
let counterMax = 20
let accuSeed = 0
let sampleCount = 1000

let counted =
    let x =
        gen {
            let! i = counter counterMin counterMax
            return Value (i, ())
        }
    x |> Gen.toListn sampleCount

let ``Sample count`` () =
    counted.Length |> should equal sampleCount

let [<TestCase>] ``Min is exclusive`` () =
    counted |> List.min |> should equal (counterMin + 1)

let [<TestCase>] ``Max is inclusive`` () =
    counted |> List.max |> should equal counterMax

let [<TestCase>] ``Incremental and reset`` () =
    let lastAndCurrent (l: 'a list) =
        l.Tail
        |> Seq.zip l
        |> Seq.toList
    counted
    |> lastAndCurrent
    |> List.map (fun (last, current) -> current = last + 1 || current = counterMin + 1 && last = counterMax)
    |> List.forall (fun x -> x = true)
    |> Assert.True
