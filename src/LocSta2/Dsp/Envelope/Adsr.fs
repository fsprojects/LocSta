module LocSta.Blocks.Dsp.Envelope.Adsr

open LocSta.Core

type AdsrPhase = Attack | Decay | Sustain | Release | Idle

/// ADSR envelope generator. Input: gate (bool). Times in seconds.
let adsr attackTime decayTime sustainLevel releaseTime =
    stream {
        let! gate = getCtx()
        let! st = useMemoWith (fun () -> MutableValue(Idle, 0.0, false))
        let (phase, level, prevGate) = st.Value

        // Detect gate transitions
        let mutable ph = phase
        let mutable lv = level
        if gate && not prevGate then
            ph <- Attack
        elif not gate && prevGate then
            ph <- Release

        match ph with
        | Attack ->
            let step = if attackTime > 0.0 then 1.0 / attackTime else 1.0
            lv <- lv + step
            if lv >= 1.0 then
                lv <- 1.0
                ph <- Decay
        | Decay ->
            let step = if decayTime > 0.0 then (1.0 - sustainLevel) / decayTime else 1.0
            lv <- lv - step
            if lv <= sustainLevel then
                lv <- sustainLevel
                ph <- Sustain
        | Sustain ->
            lv <- sustainLevel
        | Release ->
            let step = if releaseTime > 0.0 then sustainLevel / releaseTime else 1.0
            lv <- lv - step
            if lv <= 0.0 then
                lv <- 0.0
                ph <- Idle
        | Idle ->
            lv <- 0.0

        st.Value <- (ph, lv, gate)
        return lv
    }
