module LocSta.Blocks.Dsp.Analysis.PeakHold

open LocSta.Core

/// Peak hold with exponential decay. decayRate: 0.0 = hold forever, ~0.999 = slow decay.
let peakHold decayRate =
    stream {
        let! signal = getCtx()
        let! peak = useState 0.0
        let absSignal = abs signal
        if absSignal > peak.Value then
            peak.Value <- absSignal
        else
            peak.Value <- peak.Value * decayRate
        return peak.Value
    }
