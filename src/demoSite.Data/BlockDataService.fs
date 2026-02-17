namespace DemoSite.Data

open System.Collections.Generic
open LocSta.Core

module Samples =
    let noisySine n =
        [ for i in 0 .. n - 1 -> sin (float i * 0.3) + 0.3 * sin (float i * 1.7) ]

    let sine n freq =
        [ for i in 0 .. n - 1 -> sin (float i * freq) ]

    let step n at =
        [ for i in 0 .. n - 1 -> if i >= at then 1.0 else 0.0 ]

    let ramp n =
        [ for i in 0 .. n - 1 -> float i / float (n - 1) ]

    let boolPattern =
        [ false; false; true; true; false; true; false; false; true; true
          true; false; true; false; false; false; true; true; false; true ]

    let toList (xs: 'a list) = List<'a>(xs)

    let series label color values =
        { Label = label; Values = toList values; Color = color; IsStep = false }

    let stepSeries label color values =
        { Label = label; Values = toList values; Color = color; IsStep = true }

    let demo name desc snippet (inputs: ChartSeries list) (outputs: ChartSeries list) =
        { Name = name
          Description = desc
          CodeSnippet = snippet
          InputSeries = List<ChartSeries>(inputs :> ChartSeries seq)
          OutputSeries = List<ChartSeries>(outputs :> ChartSeries seq) }

open Samples

module BlockDemos =

    // ── Generators ──────────────────────────────────────────────────

    let counter () =
        let output = LocSta.Blocks.Generators.Counter.counter 0 1 |> Eval.run 20 (fun _ -> ())
        demo "Counter" "Generates incrementing integers from a start value."
            "counter 0 1 |> Eval.run 20"
            []
            [ series "Output" "#e74c3c" (output |> List.map float) ]

    let fibonacci () =
        let output = LocSta.Blocks.Generators.Fibonacci.fibonacci () |> Eval.run 15 (fun _ -> ())
        demo "Fibonacci" "Generates the Fibonacci sequence."
            "fibonacci () |> Eval.run 15"
            []
            [ series "Output" "#e74c3c" (output |> List.map float) ]

    // ── Delay ───────────────────────────────────────────────────────

    let delayBy1 () =
        let input = noisySine 20
        let output = LocSta.Blocks.Delay.DelayBy1.delayBy1 0.0 |> Eval.runWith input
        demo "DelayBy1" "Delays the signal by one sample."
            "delayBy1 0.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let delayByN () =
        let input = noisySine 25
        let output = LocSta.Blocks.Delay.DelayByN.delayByN 3 0.0 |> Eval.runWith input
        demo "DelayByN" "Delays the signal by N samples."
            "delayByN 3 0.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    // ── State ───────────────────────────────────────────────────────

    let hold () =
        let input = noisySine 20
        let output = LocSta.Blocks.State.Hold.hold (fun x -> x > 0.5) 0.0 |> Eval.runWith input
        demo "Hold" "Captures the value when predicate is true; holds it otherwise."
            "hold (fun x -> x > 0.5) 0.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let latch () =
        let triggers = [ false; true; false; false; true; false; false; true; false; false
                         false; true; false; false; false; true; false; false; false; false ]
        let values = noisySine 20
        let input = List.zip triggers values
        let output = LocSta.Blocks.State.Latch.latch 0.0 |> Eval.runWith input
        demo "Latch" "Captures the value when trigger is true; holds it until next trigger."
            "latch 0.0 |> Eval.runWith (trigger, value)"
            [ series "Value" "#3498db" values
              stepSeries "Trigger" "#95a5a6" (triggers |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ series "Output" "#e74c3c" output ]

    let edge () =
        let input = boolPattern |> List.take 15
        let output = LocSta.Blocks.State.Edge.edge () |> Eval.runWith input
        demo "Edge" "Detects rising (+1) and falling (-1) edges of a boolean signal."
            "edge () |> Eval.runWith boolInputs"
            [ stepSeries "Input" "#3498db" (input |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ stepSeries "Output" "#e74c3c" (output |> List.map float) ]

    let changed () =
        let input = [ 1; 1; 2; 2; 3; 1; 1; 1; 2; 3; 3; 4; 4; 4; 5 ]
        let output = LocSta.Blocks.State.Changed.changed () |> Eval.runWith input
        demo "Changed" "Returns true when the input value changes from the previous sample."
            "changed () |> Eval.runWith inputs"
            [ series "Input" "#3498db" (input |> List.map float) ]
            [ stepSeries "Output" "#e74c3c" (output |> List.map (fun b -> if b then 1.0 else 0.0)) ]

    // ── Arithmetic ──────────────────────────────────────────────────

    let diff () =
        let input = noisySine 20
        let output = LocSta.Blocks.Arithmetic.Diff.diff 0.0 |> Eval.runWith input
        demo "Diff" "Computes the difference between consecutive samples."
            "diff 0.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let cumulativeSum () =
        let input = [ 1.0; 2.0; -1.0; 3.0; -2.0; 1.0; 0.5; -0.5; 2.0; -1.0
                      1.5; -0.5; 0.0; 1.0; -1.0; 2.0; -2.0; 0.5; 1.0; -0.5 ]
        let output = LocSta.Blocks.Arithmetic.CumulativeSum.cumulativeSum 0.0 |> Eval.runWith input
        demo "CumulativeSum" "Running sum of all input values."
            "cumulativeSum 0.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let cumulativeProduct () =
        let input = [ 1.1; 0.9; 1.2; 0.95; 1.05; 1.1; 0.85; 1.15; 0.9; 1.1
                      1.0; 0.95; 1.05; 1.1; 0.9 ]
        let output = LocSta.Blocks.Arithmetic.CumulativeProduct.cumulativeProduct 1.0 |> Eval.runWith input
        demo "CumulativeProduct" "Running product of all input values."
            "cumulativeProduct 1.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let clamp () =
        let input = [ for i in 0..19 -> sin (float i * 0.4) * 1.5 ]
        let output = LocSta.Blocks.Arithmetic.Clamp.clamp -0.5 0.5 |> Eval.runWith input
        demo "Clamp" "Restricts the signal to a range [lo, hi]."
            "clamp -0.5 0.5 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let rescale () =
        let input = ramp 20
        let output = LocSta.Blocks.Arithmetic.Rescale.rescale 0.0 1.0 -10.0 10.0 |> Eval.runWith input
        demo "Rescale" "Maps values from one range to another."
            "rescale 0.0 1.0 -10.0 10.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    // ── Statistics ──────────────────────────────────────────────────

    let movingAverage () =
        let inputFloat = noisySine 30
        let inputInt = inputFloat |> List.map (fun x -> int (x * 10.0))
        let output = LocSta.Blocks.Statistics.MovingAverage.movingAverage 5 |> Eval.runWith inputInt
        demo "MovingAverage" "Simple moving average over the last N samples."
            "movingAverage 5 |> Eval.runWith inputs"
            [ series "Input" "#3498db" (inputInt |> List.map float) ]
            [ series "Output" "#e74c3c" output ]

    let ema () =
        let input = noisySine 30
        let output = LocSta.Blocks.Statistics.Ema.ema 0.3 |> Eval.runWith input
        demo "Ema" "Exponential moving average with smoothing factor alpha."
            "ema 0.3 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let rollingStdDev () =
        let input = noisySine 30
        let output = LocSta.Blocks.Statistics.RollingStdDev.rollingStdDev 5 |> Eval.runWith input
        demo "RollingStdDev" "Standard deviation over a sliding window."
            "rollingStdDev 5 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let rollingMin () =
        let input = noisySine 30
        let output = LocSta.Blocks.Statistics.RollingMin.rollingMin 5 |> Eval.runWith input
        demo "RollingMin" "Minimum value in a sliding window."
            "rollingMin 5 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let rollingMax () =
        let input = noisySine 30
        let output = LocSta.Blocks.Statistics.RollingMax.rollingMax 5 |> Eval.runWith input
        demo "RollingMax" "Maximum value in a sliding window."
            "rollingMax 5 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let runningMin () =
        let input = noisySine 30
        let output = LocSta.Blocks.Statistics.RunningMin.runningMin () |> Eval.runWith input
        demo "RunningMin" "All-time minimum since start."
            "runningMin () |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let runningMax () =
        let input = noisySine 30
        let output = LocSta.Blocks.Statistics.RunningMax.runningMax () |> Eval.runWith input
        demo "RunningMax" "All-time maximum since start."
            "runningMax () |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    // ── Detection ───────────────────────────────────────────────────

    let crossover () =
        let input1 = [ for i in 0..29 -> sin (float i * 0.3) ]
        let input2 = [ for i in 0..29 -> 0.0 ]
        let output =
            LocSta.Blocks.Detection.Crossover.crossover (ofSeq input1) (ofSeq input2)
            |> Eval.run 30 (fun _ -> ())
        demo "Crossover" "Detects when stream s1 crosses above (+1) or below (-1) stream s2."
            "crossover (ofSeq s1) (ofSeq s2) |> Eval.run 30"
            [ series "S1" "#3498db" input1; series "S2" "#95a5a6" input2 ]
            [ stepSeries "Output" "#e74c3c" (output |> List.map float) ]

    let threshold () =
        let input = noisySine 30
        let output = LocSta.Blocks.Detection.Threshold.threshold 0.0 |> Eval.runWith input
        demo "Threshold" "Detects when the input crosses above (+1) or below (-1) a level."
            "threshold 0.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ stepSeries "Output" "#e74c3c" (output |> List.map float) ]

    // ── Logic ───────────────────────────────────────────────────────

    let debounce () =
        let input = boolPattern
        let output = LocSta.Blocks.Logic.Debounce.debounce 3 |> Eval.runWith input
        demo "Debounce" "Only changes output after N consecutive identical inputs."
            "debounce 3 |> Eval.runWith boolInputs"
            [ stepSeries "Input" "#3498db" (input |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ stepSeries "Output" "#e74c3c" (output |> List.map (fun b -> if b then 1.0 else 0.0)) ]

    let toggle () =
        let input = boolPattern
        let output = LocSta.Blocks.Logic.Toggle.toggle () |> Eval.runWith input
        demo "Toggle" "Toggles state on each true input (like a light switch)."
            "toggle () |> Eval.runWith boolInputs"
            [ stepSeries "Input" "#3498db" (input |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ stepSeries "Output" "#e74c3c" (output |> List.map (fun b -> if b then 1.0 else 0.0)) ]

    // ── Counting ────────────────────────────────────────────────────

    let countWhere () =
        let input = noisySine 20
        let output = LocSta.Blocks.Counting.CountWhere.countWhere (fun x -> x > 0.0) |> Eval.runWith input
        demo "CountWhere" "Counts samples matching a predicate."
            "countWhere (fun x -> x > 0.0) |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" (output |> List.map float) ]

    let countSince () =
        let input = boolPattern
        let output = LocSta.Blocks.Counting.CountSince.countSince () |> Eval.runWith input
        demo "CountSince" "Counts samples since the last true trigger."
            "countSince () |> Eval.runWith boolInputs"
            [ stepSeries "Input" "#3498db" (input |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ series "Output" "#e74c3c" (output |> List.map float) ]

    let countIn () =
        let input = boolPattern
        let output = LocSta.Blocks.Counting.CountIn.countIn 5 |> Eval.runWith input
        demo "CountIn" "Counts true values in a sliding window."
            "countIn 5 |> Eval.runWith boolInputs"
            [ stepSeries "Input" "#3498db" (input |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ series "Output" "#e74c3c" (output |> List.map float) ]

    let timeSince () =
        let triggers = [ false; false; true; false; false; false; true; false; false; false
                         false; true; false; false; false ]
        let times = [ for i in 0..14 -> float i * 0.1 ]
        let input = List.zip triggers times
        let output = LocSta.Blocks.Counting.TimeSince.timeSince () |> Eval.runWith input
        demo "TimeSince" "Elapsed time since the last trigger."
            "timeSince () |> Eval.runWith (trigger, time)"
            [ stepSeries "Trigger" "#95a5a6" (triggers |> List.map (fun b -> if b then 1.0 else 0.0))
              series "Time" "#3498db" times ]
            [ series "Output" "#e74c3c" output ]

    let rate () =
        let input = boolPattern
        let output = LocSta.Blocks.Counting.Rate.rate 5 |> Eval.runWith input
        demo "Rate" "Ratio of true values in a sliding window (0.0 to 1.0)."
            "rate 5 |> Eval.runWith boolInputs"
            [ stepSeries "Input" "#3498db" (input |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ series "Output" "#e74c3c" output ]

    // ── Windowing ───────────────────────────────────────────────────

    let windowedReduce () =
        let input = noisySine 20
        let output = LocSta.Blocks.Windowing.WindowedReduce.windowedReduce 5 (+) 0.0 |> Eval.runWith input
        demo "WindowedReduce" "Applies a fold over a sliding window of samples."
            "windowedReduce 5 (+) 0.0 |> Eval.runWith inputs"
            [ series "Input" "#3498db" input ]
            [ series "Output" "#e74c3c" output ]

    let segment () =
        let values = [ 1; 2; 3; 4; 5; 6; 7; 8; 9; 10 ]
        let boundaries = [ false; false; true; false; true; false; false; false; true; true ]
        let input = List.zip values boundaries
        let output = LocSta.Blocks.Windowing.Segment.segment () |> Eval.runWith input
        demo "Segment" "Accumulates values into segments delimited by a boundary flag."
            "segment () |> Eval.runWith (value, boundary)"
            [ series "Value" "#3498db" (values |> List.map float)
              stepSeries "Boundary" "#95a5a6" (boundaries |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ stepSeries "Output" "#e74c3c" (output |> List.map (fun o -> match o with Some _ -> 1.0 | None -> 0.0)) ]

    // ── DSP: Oscillators ────────────────────────────────────────────

    let sineOsc () =
        let sr = 100.0
        let n = 200
        let input = List.replicate n 2.0
        let output = LocSta.Blocks.Dsp.Oscillators.SineOsc.sineOsc sr |> Eval.runWith input
        demo "SineOsc" "Sine wave oscillator."
            "sineOsc 100.0 |> Eval.runWith (replicate 200 2.0)"
            []
            [ series "Output" "#e74c3c" output ]

    let sawOsc () =
        let sr = 100.0
        let n = 200
        let input = List.replicate n 2.0
        let output = LocSta.Blocks.Dsp.Oscillators.SawOsc.sawOsc sr |> Eval.runWith input
        demo "SawOsc" "Sawtooth wave oscillator."
            "sawOsc 100.0 |> Eval.runWith (replicate 200 2.0)"
            []
            [ series "Output" "#e74c3c" output ]

    let squareOsc () =
        let sr = 100.0
        let n = 200
        let input = List.replicate n (2.0, 0.5)
        let output = LocSta.Blocks.Dsp.Oscillators.SquareOsc.squareOsc sr |> Eval.runWith input
        demo "SquareOsc" "Square wave oscillator with pulse width control."
            "squareOsc 100.0 |> Eval.runWith (replicate 200 (2.0, 0.5))"
            []
            [ stepSeries "Output" "#e74c3c" output ]

    let triangleOsc () =
        let sr = 100.0
        let n = 200
        let input = List.replicate n 2.0
        let output = LocSta.Blocks.Dsp.Oscillators.TriangleOsc.triangleOsc sr |> Eval.runWith input
        demo "TriangleOsc" "Triangle wave oscillator."
            "triangleOsc 100.0 |> Eval.runWith (replicate 200 2.0)"
            []
            [ series "Output" "#e74c3c" output ]

    let whiteNoise () =
        let n = 200
        let input = List.replicate n ()
        let output = LocSta.Blocks.Dsp.Oscillators.WhiteNoise.whiteNoise 42 |> Eval.runWith input
        demo "WhiteNoise" "White noise generator (seeded)."
            "whiteNoise 42 |> Eval.runWith (replicate 200 ())"
            []
            [ series "Output" "#e74c3c" output ]

    // ── DSP: Filters ────────────────────────────────────────────────

    let lowPass1 () =
        let sr = 1000.0
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) + 0.5 * sin (float i * 0.8) ]
        let cutoff = 50.0
        let input = signal |> List.map (fun s -> (s, cutoff))
        let output = LocSta.Blocks.Dsp.Filters.LowPass1.lowPass1 sr |> Eval.runWith input
        demo "LowPass1" "1-pole low-pass filter."
            "lowPass1 1000.0 |> Eval.runWith (signal, 50.0)"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let highPass1 () =
        let sr = 1000.0
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) + 0.5 * sin (float i * 0.8) ]
        let cutoff = 50.0
        let input = signal |> List.map (fun s -> (s, cutoff))
        let output = LocSta.Blocks.Dsp.Filters.HighPass1.highPass1 sr |> Eval.runWith input
        demo "HighPass1" "1-pole high-pass filter."
            "highPass1 1000.0 |> Eval.runWith (signal, 50.0)"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let biquadLowPass () =
        let sr = 1000.0
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) + 0.5 * sin (float i * 0.8) ]
        let input = signal |> List.map (fun s -> (s, 50.0, 0.707))
        let output = LocSta.Blocks.Dsp.Filters.BiquadLowPass.biquadLowPass sr |> Eval.runWith input
        demo "BiquadLowPass" "Biquad low-pass filter (2nd order)."
            "biquadLowPass 1000.0 |> Eval.runWith (signal, 50.0, 0.707)"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let biquadHighPass () =
        let sr = 1000.0
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) + 0.5 * sin (float i * 0.8) ]
        let input = signal |> List.map (fun s -> (s, 50.0, 0.707))
        let output = LocSta.Blocks.Dsp.Filters.BiquadHighPass.biquadHighPass sr |> Eval.runWith input
        demo "BiquadHighPass" "Biquad high-pass filter (2nd order)."
            "biquadHighPass 1000.0 |> Eval.runWith (signal, 50.0, 0.707)"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let biquadBandPass () =
        let sr = 1000.0
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) + 0.5 * sin (float i * 0.8) ]
        let input = signal |> List.map (fun s -> (s, 80.0, 1.0))
        let output = LocSta.Blocks.Dsp.Filters.BiquadBandPass.biquadBandPass sr |> Eval.runWith input
        demo "BiquadBandPass" "Biquad band-pass filter (2nd order)."
            "biquadBandPass 1000.0 |> Eval.runWith (signal, 80.0, 1.0)"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let dcBlock () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) + 2.0 ]
        let output = LocSta.Blocks.Dsp.Filters.DcBlock.dcBlock 0.995 |> Eval.runWith signal
        demo "DcBlock" "Removes DC offset from a signal."
            "dcBlock 0.995 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    // ── DSP: Envelope ───────────────────────────────────────────────

    let envFollow () =
        let sr = 1000.0
        let n = 200
        let signal = [ for i in 0 .. n - 1 ->
                            let env = if i < 80 then 1.0 elif i < 120 then 0.0 else 0.7
                            env * sin (float i * 0.5) ]
        let output = LocSta.Blocks.Dsp.Envelope.EnvFollow.envFollow sr 5.0 50.0 |> Eval.runWith signal
        demo "EnvFollow" "Envelope follower with separate attack and release times."
            "envFollow 1000.0 5.0 50.0 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let adsr () =
        let n = 60
        let gate = [ for i in 0 .. n - 1 -> i >= 5 && i < 35 ]
        let output = LocSta.Blocks.Dsp.Envelope.Adsr.adsr 5.0 5.0 0.5 10.0 |> Eval.runWith gate
        demo "Adsr" "ADSR envelope generator. Input: gate (bool)."
            "adsr 5.0 5.0 0.5 10.0 |> Eval.runWith gate"
            [ stepSeries "Gate" "#3498db" (gate |> List.map (fun b -> if b then 1.0 else 0.0)) ]
            [ series "Output" "#e74c3c" output ]

    // ── DSP: Dynamics ───────────────────────────────────────────────

    let softClip () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) * 3.0 ]
        let output = LocSta.Blocks.Dsp.Dynamics.SoftClip.softClip 3.0 |> Eval.runWith signal
        demo "SoftClip" "Soft-clips the signal with a tanh curve."
            "softClip 3.0 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let hardClip () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.1) * 1.5 ]
        let output = LocSta.Blocks.Dsp.Dynamics.HardClip.hardClip 0.5 |> Eval.runWith signal
        demo "HardClip" "Hard-clips the signal at a threshold."
            "hardClip 0.5 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let gate () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.3) ]
        let envelope = [ for i in 0 .. n - 1 -> abs (sin (float i * 0.05)) ]
        let input = List.zip signal envelope
        let output = LocSta.Blocks.Dsp.Dynamics.Gate.gate 0.3 |> Eval.runWith input
        demo "Gate" "Noise gate: silences signal when envelope is below threshold."
            "gate 0.3 |> Eval.runWith (signal, envelope)"
            [ series "Signal" "#3498db" signal; series "Envelope" "#95a5a6" envelope ]
            [ series "Output" "#e74c3c" output ]

    // ── DSP: Modulation ─────────────────────────────────────────────

    let ringMod () =
        let n = 200
        let sr = 100.0
        let s1Data = [ for i in 0 .. n - 1 -> sin (float i * 2.0 * System.Math.PI * 3.0 / sr) ]
        let s2Data = [ for i in 0 .. n - 1 -> sin (float i * 2.0 * System.Math.PI * 15.0 / sr) ]
        let output =
            LocSta.Blocks.Dsp.Modulation.RingMod.ringMod (ofSeq s1Data) (ofSeq s2Data)
            |> Eval.run n (fun _ -> ())
        demo "RingMod" "Ring modulator: multiplies two signals."
            "ringMod s1 s2 |> Eval.run 200"
            [ series "S1" "#3498db" s1Data; series "S2" "#95a5a6" s2Data ]
            [ series "Output" "#e74c3c" output ]

    let bitCrush () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 -> sin (float i * 0.05) ]
        let output = LocSta.Blocks.Dsp.Modulation.BitCrush.bitCrush 3 |> Eval.runWith signal
        demo "BitCrush" "Bit crusher: reduces bit depth for lo-fi effect."
            "bitCrush 3 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let crossfade () =
        let n = 100
        let sr = 100.0
        let s1Data = [ for i in 0 .. n - 1 -> sin (float i * 2.0 * System.Math.PI * 2.0 / sr) ]
        let s2Data = [ for i in 0 .. n - 1 -> sin (float i * 2.0 * System.Math.PI * 8.0 / sr) ]
        let mixData = [ for i in 0 .. n - 1 -> float i / float (n - 1) ]
        let output =
            LocSta.Blocks.Dsp.Modulation.Crossfade.crossfade (ofSeq s1Data) (ofSeq s2Data) (ofSeq mixData)
            |> Eval.run n (fun _ -> ())
        demo "Crossfade" "Crossfades between two signals using a mix parameter (0=s1, 1=s2)."
            "crossfade s1 s2 mix |> Eval.run 100"
            [ series "S1" "#3498db" s1Data; series "S2" "#95a5a6" s2Data
              series "Mix" "#2ecc71" mixData ]
            [ series "Output" "#e74c3c" output ]

    // ── DSP: Analysis ───────────────────────────────────────────────

    let rms () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 ->
                            let env = if i < 80 then 1.0 elif i < 120 then 0.3 else 0.8
                            env * sin (float i * 0.3) ]
        let output = LocSta.Blocks.Dsp.Analysis.Rms.rms 20 |> Eval.runWith signal
        demo "Rms" "Root mean square level over a sliding window."
            "rms 20 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let zeroCrossRate () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 ->
                            let freq = if i < 100 then 0.1 else 0.5
                            sin (float i * freq) ]
        let output = LocSta.Blocks.Dsp.Analysis.ZeroCrossRate.zeroCrossRate 20 |> Eval.runWith signal
        demo "ZeroCrossRate" "Rate of zero crossings in a sliding window."
            "zeroCrossRate 20 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]

    let peakHold () =
        let n = 200
        let signal = [ for i in 0 .. n - 1 ->
                            let burst = if (i / 50) % 2 = 0 then 1.0 else 0.3
                            burst * abs (sin (float i * 0.2)) ]
        let output = LocSta.Blocks.Dsp.Analysis.PeakHold.peakHold 0.95 |> Eval.runWith signal
        demo "PeakHold" "Tracks the peak level with exponential decay."
            "peakHold 0.95 |> Eval.runWith signal"
            [ series "Input" "#3498db" signal ]
            [ series "Output" "#e74c3c" output ]


type BlockDataService() =
    let categories =
        let cat name slug (blocks: BlockDemo list) =
            { Name = name; Slug = slug; Blocks = List<BlockDemo>(blocks :> BlockDemo seq) }
        List<Category>(
            [
                cat "Generators" "generators"
                    [ BlockDemos.counter(); BlockDemos.fibonacci() ]
                cat "Delay" "delay"
                    [ BlockDemos.delayBy1(); BlockDemos.delayByN() ]
                cat "State" "state"
                    [ BlockDemos.hold(); BlockDemos.latch(); BlockDemos.edge(); BlockDemos.changed() ]
                cat "Arithmetic" "arithmetic"
                    [ BlockDemos.diff(); BlockDemos.cumulativeSum(); BlockDemos.cumulativeProduct()
                      BlockDemos.clamp(); BlockDemos.rescale() ]
                cat "Statistics" "statistics"
                    [ BlockDemos.movingAverage(); BlockDemos.ema(); BlockDemos.rollingStdDev()
                      BlockDemos.rollingMin(); BlockDemos.rollingMax()
                      BlockDemos.runningMin(); BlockDemos.runningMax() ]
                cat "Detection" "detection"
                    [ BlockDemos.crossover(); BlockDemos.threshold() ]
                cat "Logic" "logic"
                    [ BlockDemos.debounce(); BlockDemos.toggle() ]
                cat "Counting" "counting"
                    [ BlockDemos.countWhere(); BlockDemos.countSince(); BlockDemos.countIn()
                      BlockDemos.timeSince(); BlockDemos.rate() ]
                cat "Windowing" "windowing"
                    [ BlockDemos.windowedReduce(); BlockDemos.segment() ]
                cat "DSP: Oscillators" "dsp-oscillators"
                    [ BlockDemos.sineOsc(); BlockDemos.sawOsc(); BlockDemos.squareOsc()
                      BlockDemos.triangleOsc(); BlockDemos.whiteNoise() ]
                cat "DSP: Filters" "dsp-filters"
                    [ BlockDemos.lowPass1(); BlockDemos.highPass1()
                      BlockDemos.biquadLowPass(); BlockDemos.biquadHighPass(); BlockDemos.biquadBandPass()
                      BlockDemos.dcBlock() ]
                cat "DSP: Envelope" "dsp-envelope"
                    [ BlockDemos.envFollow(); BlockDemos.adsr() ]
                cat "DSP: Dynamics" "dsp-dynamics"
                    [ BlockDemos.softClip(); BlockDemos.hardClip(); BlockDemos.gate() ]
                cat "DSP: Modulation" "dsp-modulation"
                    [ BlockDemos.ringMod(); BlockDemos.bitCrush(); BlockDemos.crossfade() ]
                cat "DSP: Analysis" "dsp-analysis"
                    [ BlockDemos.rms(); BlockDemos.zeroCrossRate(); BlockDemos.peakHold() ]
            ]
        )
    member _.Categories = categories
