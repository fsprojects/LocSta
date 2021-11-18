module TestHelper

open FsUnit

let inline equals (expected: 'a) (actual: 'a) = actual |> should equal expected
