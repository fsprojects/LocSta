module LocSta.Blocks.Dsp.Envelope.Adsr

open LocSta.Core

type AdsrPhase = Attack | Decay | Sustain | Release | Idle

/// ADSR envelope generator. Input: gate (bool). Times in seconds.
let adsr attackTime decayTime sustainLevel releaseTime =
    stream {
        let! gate = getCtx()
        let! phase = useState Idle
        let! level = useState 0.0
        let! prevGate = useState false

        // Detect gate transitions
        if gate && not prevGate.Value then
            phase.Value <- Attack
        elif not gate && prevGate.Value then
            phase.Value <- Release
        prevGate.Value <- gate

        match phase.Value with
        | Attack ->
            let step = if attackTime > 0.0 then 1.0 / attackTime else 1.0
            level.Value <- level.Value + step
            if level.Value >= 1.0 then
                level.Value <- 1.0
                phase.Value <- Decay
        | Decay ->
            let step = if decayTime > 0.0 then (1.0 - sustainLevel) / decayTime else 1.0
            level.Value <- level.Value - step
            if level.Value <= sustainLevel then
                level.Value <- sustainLevel
                phase.Value <- Sustain
        | Sustain ->
            level.Value <- sustainLevel
        | Release ->
            let step = if releaseTime > 0.0 then sustainLevel / releaseTime else 1.0
            level.Value <- level.Value - step
            if level.Value <= 0.0 then
                level.Value <- 0.0
                phase.Value <- Idle
        | Idle ->
            level.Value <- 0.0

        return level.Value
    }
