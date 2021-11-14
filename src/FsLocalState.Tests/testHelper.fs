module TestHelper

open FsUnit

let inline equals (expected: 'a) (actual: 'b) = actual |> should equal expected
