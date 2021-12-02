import { Gen_toListn, Feed_EmitMany$2, Gen_FeedBuilder__Return_Z6686A729, Gen_LoopBuilder__Zero, Loop_Stop, Gen_LoopBuilder__Return_2CC951C7, TopLevelOperators_feed, Gen_map2, TopLevelOperators_loop, Gen_BaseBuilder__ReturnFrom_1505, Gen_loop, Gen_LoopBuilder__Yield_1505, Gen_feed, Gen_FeedBuilder__Yield_2A0A0, Init$1, Gen_FeedBuilder__Bind_Z7B4A5563, Res_Loop_skipAndReset, Gen_LoopBuilder__Bind_Z7EFF1A0D, Gen_BaseBuilder__Delay_1505, Gen_BaseBuilder__Run_FCFD9EF, Res_Loop_emitManyAndReset, Gen_bind, Gen_createLoop, Res$2, Gen_run } from "./core.fs.js";
import { exactlyOne, cons, singleton, append, empty, length, takeWhile } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/List.js";
import { comparePrimitives, max, compare } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Util.js";
import { singleton as singleton_1, collect, delay, toList } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Seq.js";
import { defaultArg, some } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Option.js";
import { class_type } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Reflection.js";

function Lib_Gen_resetFuncForMapValue(gen, _arg2, _arg1) {
    return Gen_run(gen)(void 0);
}

function Lib_Gen_stopFuncForMapValue(_arg2, values, _arg1) {
    return new Res$2(1, values);
}

export function Lib_Gen_whenFuncThen(pred, onTrue, inputGen) {
    return Gen_createLoop((state) => {
        const matchValue = Gen_run(inputGen)(state);
        if (matchValue.tag === 1) {
            const values_1 = matchValue.fields[0];
            return new Res$2(1, values_1);
        }
        else {
            const values = matchValue.fields[0];
            const state_1 = matchValue.fields[1];
            const valuesUntilPred = takeWhile((v) => (!pred(v)), values);
            return (length(valuesUntilPred) === length(values)) ? (new Res$2(0, values, state_1)) : onTrue(inputGen, valuesUntilPred, state_1);
        }
    });
}

export function Lib_Gen_whenFuncThenReset(pred, inputGen) {
    return Lib_Gen_whenFuncThen(pred, (gen, arg10$0040, arg20$0040) => Lib_Gen_resetFuncForMapValue(gen, arg10$0040, arg20$0040), inputGen);
}

export function Lib_Gen_whenFuncThenStop(pred, inputGen) {
    return Lib_Gen_whenFuncThen(pred, (arg00$0040, values, arg20$0040) => Lib_Gen_stopFuncForMapValue(arg00$0040, values, arg20$0040), inputGen);
}

export function Lib_Gen_whenValueThen(pred, onTrue, inputGen) {
    return Lib_Gen_whenFuncThen((_arg1) => pred, onTrue, inputGen);
}

export function Lib_Gen_whenValueThenReset(pred, inputGen) {
    return Lib_Gen_whenValueThen(pred, (gen, arg10$0040, arg20$0040) => Lib_Gen_resetFuncForMapValue(gen, arg10$0040, arg20$0040), inputGen);
}

export function Lib_Gen_whenValueThenStop(pred, inputGen) {
    return Lib_Gen_whenValueThen(pred, (arg00$0040, values, arg20$0040) => Lib_Gen_stopFuncForMapValue(arg00$0040, values, arg20$0040), inputGen);
}

export function Lib_Gen_whenLoopThen(pred, onTrue, inputGen) {
    return Gen_bind((pred_1) => Lib_Gen_whenValueThen(pred_1, onTrue, inputGen), pred);
}

export function Lib_Gen_whenLoopThenReset(pred, inputGen) {
    return Lib_Gen_whenLoopThen(pred, (gen, arg10$0040, arg20$0040) => Lib_Gen_resetFuncForMapValue(gen, arg10$0040, arg20$0040), inputGen);
}

export function Lib_Gen_whenLoopThenStop(pred, inputGen) {
    return Lib_Gen_whenLoopThen(pred, (arg00$0040, values, arg20$0040) => Lib_Gen_stopFuncForMapValue(arg00$0040, values, arg20$0040), inputGen);
}

export function Lib_Gen_onStopThenReset(inputGen) {
    return Gen_createLoop((state) => {
        const matchValue = Gen_run(inputGen)(state);
        if (matchValue.tag === 1) {
            const values = matchValue.fields[0];
            return Res_Loop_emitManyAndReset(values);
        }
        else {
            const x = matchValue;
            return x;
        }
    });
}

export function Lib_Gen_onCountThen(count, onTrue, inputGen) {
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_loop, Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => {
        let inclusiveEnd, onEnd;
        return Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, (inclusiveEnd = ((count - 1) | 0), (onEnd = Res_Loop_skipAndReset(), Gen_BaseBuilder__Run_FCFD9EF(Gen_loop, Gen_BaseBuilder__Delay_1505(Gen_loop, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(Gen_loop, Gen_BaseBuilder__Run_FCFD9EF(Gen_feed, Gen_BaseBuilder__Delay_1505(Gen_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(Gen_feed, new Init$1(0, 0), (_arg1) => {
            const curr = _arg1;
            const tupledArg = [curr, curr + 1];
            return Gen_FeedBuilder__Yield_2A0A0(Gen_feed, tupledArg[0], tupledArg[1]);
        }))), (_arg1_1) => {
            const c = _arg1_1;
            return (compare(c, inclusiveEnd) <= 0) ? Gen_LoopBuilder__Yield_1505(Gen_loop, c) : Gen_BaseBuilder__ReturnFrom_1505(Gen_loop, Gen_createLoop((_arg1_2) => onEnd));
        }))))), (_arg1_3) => {
            const c_1 = _arg1_3 | 0;
            return Gen_BaseBuilder__ReturnFrom_1505(TopLevelOperators_loop, Lib_Gen_whenValueThen(c_1 === count, onTrue, inputGen));
        });
    }));
}

export function Lib_Gen_onCountThenReset(count, inputGen) {
    return Lib_Gen_onCountThen(count, (gen, arg10$0040, arg20$0040) => Lib_Gen_resetFuncForMapValue(gen, arg10$0040, arg20$0040), inputGen);
}

export function Lib_Gen_onCountThenStop(count, inputGen) {
    return Lib_Gen_onCountThen(count, (arg00$0040, values, arg20$0040) => Lib_Gen_stopFuncForMapValue(arg00$0040, values, arg20$0040), inputGen);
}

export function Lib_Gen_includeState(inputGen) {
    return Gen_map2((v, s) => [v, s], inputGen);
}

export function Lib_Gen_accumulate(currentValue) {
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_feed, Gen_BaseBuilder__Delay_1505(TopLevelOperators_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(TopLevelOperators_feed, new Init$1(0, empty()), (_arg1) => {
        const elements = _arg1;
        const newElements = append(elements, singleton(currentValue));
        const tupledArg = [newElements, newElements];
        return Gen_FeedBuilder__Yield_2A0A0(TopLevelOperators_feed, tupledArg[0], tupledArg[1]);
    })));
}

export function Lib_Gen_accumulateOnePart(partLength, currentValue) {
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_loop, Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, Gen_BaseBuilder__Run_FCFD9EF(Gen_feed, Gen_BaseBuilder__Delay_1505(Gen_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(Gen_feed, new Init$1(0, 0), (_arg1) => {
        const curr = _arg1;
        const tupledArg = [curr, curr + 1];
        return Gen_FeedBuilder__Yield_2A0A0(Gen_feed, tupledArg[0], tupledArg[1]);
    }))), (_arg1_1) => {
        const c = _arg1_1 | 0;
        return Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, Lib_Gen_accumulate(currentValue), (_arg2) => {
            const acc = _arg2;
            return (c === (partLength - 1)) ? Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, acc) : ((c === partLength) ? Gen_LoopBuilder__Return_2CC951C7(TopLevelOperators_loop, new Loop_Stop(0)) : Gen_LoopBuilder__Zero(TopLevelOperators_loop));
        });
    })));
}

export function Lib_Gen_accumulateManyParts(count, currentValue) {
    return Lib_Gen_onStopThenReset(Lib_Gen_accumulateOnePart(count, currentValue));
}

export function Lib_Gen_fork(inputGen) {
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_feed, Gen_BaseBuilder__Delay_1505(TopLevelOperators_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(TopLevelOperators_feed, new Init$1(0, empty()), (_arg1) => {
        const runningStates = _arg1;
        const inputGen_1 = Gen_run(inputGen);
        let resultValues = empty();
        const newForkStates = toList(delay(() => collect((forkState) => {
            const matchValue = inputGen_1(forkState);
            if (matchValue.tag === 1) {
                const values_1 = matchValue.fields[0];
                resultValues = append(resultValues, values_1);
                return singleton_1(void 0);
            }
            else {
                const values = matchValue.fields[0];
                const s = matchValue.fields[1];
                resultValues = append(resultValues, values);
                return singleton_1(some(s));
            }
        }, cons(void 0, runningStates))));
        return Gen_FeedBuilder__Return_Z6686A729(TopLevelOperators_feed, new Feed_EmitMany$2(0, resultValues, newForkStates));
    })));
}

function Lib_Gen_dotnetRandom() {
    return {};
}

export function Lib_Gen_random() {
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_feed, Gen_BaseBuilder__Delay_1505(TopLevelOperators_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(TopLevelOperators_feed, new Init$1(0, Lib_Gen_dotnetRandom()), (_arg1) => {
        const random = _arg1;
        const tupledArg = [Math.random(), random];
        return Gen_FeedBuilder__Yield_2A0A0(TopLevelOperators_feed, tupledArg[0], tupledArg[1]);
    })));
}

export function Lib_Gen_head(g) {
    return exactlyOne(Gen_toListn(1, g));
}

export function Lib_Gen_skip(n, g) {
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_loop, Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, g, (_arg1) => {
        const v = _arg1;
        return Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, Gen_BaseBuilder__Run_FCFD9EF(Gen_feed, Gen_BaseBuilder__Delay_1505(Gen_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(Gen_feed, new Init$1(0, 0), (_arg1_1) => {
            const curr = _arg1_1;
            const tupledArg = [curr, curr + 1];
            return Gen_FeedBuilder__Yield_2A0A0(Gen_feed, tupledArg[0], tupledArg[1]);
        }))), (_arg2) => {
            const c = _arg2 | 0;
            return (c >= n) ? Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, v) : Gen_LoopBuilder__Zero(TopLevelOperators_loop);
        });
    })));
}

export function Lib_Gen_take(n, g) {
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_loop, Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, g, (_arg1) => {
        const v = _arg1;
        return Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, Gen_BaseBuilder__Run_FCFD9EF(Gen_feed, Gen_BaseBuilder__Delay_1505(Gen_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(Gen_feed, new Init$1(0, 0), (_arg1_1) => {
            const curr = _arg1_1;
            const tupledArg = [curr, curr + 1];
            return Gen_FeedBuilder__Yield_2A0A0(Gen_feed, tupledArg[0], tupledArg[1]);
        }))), (_arg2) => {
            const c = _arg2 | 0;
            return (c < n) ? Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, v) : Gen_LoopBuilder__Return_2CC951C7(TopLevelOperators_loop, new Loop_Stop(0));
        });
    })));
}

export class Extensions {
    constructor() {
    }
}

export function Extensions$reflection() {
    return class_type("LocSta.Extensions", void 0, Extensions);
}

export function Extensions_$ctor() {
    return new Extensions();
}

export function Extensions_GetSlice_Z2F4515A1(inputGen, inclStartIdx, inclEndIdx) {
    const s = max((x, y) => comparePrimitives(x, y), 0, defaultArg(inclStartIdx, 0)) | 0;
    return Gen_BaseBuilder__Run_FCFD9EF(TopLevelOperators_loop, Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, Gen_BaseBuilder__Run_FCFD9EF(Gen_feed, Gen_BaseBuilder__Delay_1505(Gen_feed, () => Gen_FeedBuilder__Bind_Z7B4A5563(Gen_feed, new Init$1(0, 0), (_arg1) => {
        const curr = _arg1;
        const tupledArg = [curr, curr + 1];
        return Gen_FeedBuilder__Yield_2A0A0(Gen_feed, tupledArg[0], tupledArg[1]);
    }))), (_arg1_1) => {
        const i = _arg1_1 | 0;
        return Gen_LoopBuilder__Bind_Z7EFF1A0D(TopLevelOperators_loop, inputGen, (_arg2) => {
            let e, e_1;
            const value = _arg2;
            return (i >= s) ? ((inclEndIdx != null) ? (((e = (inclEndIdx | 0), i > e)) ? ((e_1 = (inclEndIdx | 0), Gen_LoopBuilder__Return_2CC951C7(TopLevelOperators_loop, new Loop_Stop(0)))) : Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, value)) : Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, value)) : Gen_LoopBuilder__Zero(TopLevelOperators_loop);
        });
    })));
}

