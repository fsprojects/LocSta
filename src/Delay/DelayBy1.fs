module LocSta.Blocks.Delay.DelayBy1

open LocSta.Blocks.Delay.DelayByN

/// Delays the input stream by 1 sample, returning 'defaultValue' on the first evaluation.
let delayBy1 defaultValue = delayByN 1 defaultValue
