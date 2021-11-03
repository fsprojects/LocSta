
#r "../FsLocalState.Core/bin/Debug/netstandard2.0/FsLocalState.dll"
open FsLocalState

gen {
    let! c = count01
    return Control.Feedback (c, 0)
}
