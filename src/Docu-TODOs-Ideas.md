
* yield and return: document design decisions in combination with let! and for (unconventional)

* if-else
    * Alternative:
        if state = n then
            yield vf
            return Feed.ResetThis
        if state <> n then
            yield vf

* Unterschied erklären zwischen
    * let! v = [0..10] |> Gen.ofList
    * for v in [0..10] do
