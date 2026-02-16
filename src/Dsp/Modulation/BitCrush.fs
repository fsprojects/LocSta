module LocSta.Blocks.Dsp.Modulation.BitCrush

open LocSta.Core

/// Bit crusher: reduces bit depth. bits = number of quantization levels.
let bitCrush bits =
    stream {
        let! signal = getCtx()
        let levels = pown 2.0 bits
        return floor (signal * levels + 0.5) / levels
    }
