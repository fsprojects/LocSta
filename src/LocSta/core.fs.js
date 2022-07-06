import { Record, Union } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Types.js";
import { class_type, record_type, bool_type, unit_type, list_type, union_type, lambda_type, option_type } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Reflection.js";
import { exactlyOne, cons, append, head, tail, isEmpty, empty, singleton } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/List.js";
import { defaultArg, defaultArgWith, some } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Option.js";
import { comparePrimitives, max, getEnumerator } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Util.js";
import { singleton as singleton_1, collect, map, truncate, toList, enumerateWhile, delay } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Seq.js";

export class Gen$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Gen"];
    }
}

export function Gen$2$reflection(gen0, gen1) {
    return union_type("LocSta.Gen`2", [gen0, gen1], Gen$2, () => [[["Item", lambda_type(option_type(gen1), gen0)]]]);
}

export class Res$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Continue", "Stop"];
    }
}

export function Res$2$reflection(gen0, gen1) {
    return union_type("LocSta.Res`2", [gen0, gen1], Res$2, () => [[["Item1", list_type(gen0)], ["Item2", gen1]], [["Item", list_type(gen0)]]]);
}

export class LoopState$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Update", "KeepLast", "Reset"];
    }
}

export function LoopState$1$reflection(gen0) {
    return union_type("LocSta.LoopState`1", [gen0], LoopState$1, () => [[["Item", gen0]], [], []]);
}

export class FeedType$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Update", "KeepLast", "Reset", "ResetFeedback", "ResetDescendants"];
    }
}

export function FeedType$1$reflection(gen0) {
    return union_type("LocSta.FeedType`1", [gen0], FeedType$1, () => [[["Item", gen0]], [], [], [], [["Item", gen0]]]);
}

export class FeedState$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["FeedState"];
    }
}

export function FeedState$2$reflection(gen0, gen1) {
    return union_type("LocSta.FeedState`2", [gen0, gen1], FeedState$2, () => [[["Item1", option_type(gen0)], ["Item2", FeedType$1$reflection(gen1)]]]);
}

export class Init$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Init", "InitWith"];
    }
}

export function Init$1$reflection(gen0) {
    return union_type("LocSta.Init`1", [gen0], Init$1, () => [[["Item", gen0]], [["Item", lambda_type(unit_type, gen0)]]]);
}

export class BindState$3 extends Record {
    constructor(mstate, kstate, mleftovers, isStopped) {
        super();
        this.mstate = mstate;
        this.kstate = kstate;
        this.mleftovers = mleftovers;
        this.isStopped = isStopped;
    }
}

export function BindState$3$reflection(gen0, gen1, gen2) {
    return record_type("LocSta.BindState`3", [gen0, gen1, gen2], BindState$3, () => [["mstate", option_type(gen0)], ["kstate", option_type(gen1)], ["mleftovers", list_type(gen2)], ["isStopped", bool_type]]);
}

export function Res_Loop_emitMany(values, state) {
    return new Res$2(0, values, new LoopState$1(0, state));
}

export function Res_Loop_emitManyAndKeepLast(values) {
    return new Res$2(0, values, new LoopState$1(1));
}

export function Res_Loop_emitManyAndReset(values) {
    return new Res$2(0, values, new LoopState$1(2));
}

export function Res_Loop_emitManyAndStop(values) {
    return new Res$2(1, values);
}

export function Res_Loop_emit(value, state) {
    return Res_Loop_emitMany(singleton(value), state);
}

export function Res_Loop_emitAndKeepLast(value) {
    return Res_Loop_emitManyAndKeepLast(singleton(value));
}

export function Res_Loop_emitAndReset(value) {
    return Res_Loop_emitManyAndReset(singleton(value));
}

export function Res_Loop_emitAndStop(value) {
    return Res_Loop_emitManyAndStop(singleton(value));
}

export function Res_Loop_skip(state) {
    return Res_Loop_emitMany(empty(), state);
}

export function Res_Loop_skipAndKeepLast() {
    return Res_Loop_emitManyAndKeepLast(empty());
}

export function Res_Loop_skipAndReset() {
    return Res_Loop_emitManyAndReset(empty());
}

export function Res_Loop_stop() {
    return Res_Loop_emitManyAndStop(empty());
}

export function Res_Loop_zero() {
    return Res_Loop_skipAndKeepLast();
}

export function Res_Feed_emitMany(values, feedback) {
    return new Res$2(0, values, new FeedState$2(0, void 0, new FeedType$1(0, feedback)));
}

export function Res_Feed_emitManyAndKeepLast(values) {
    return new Res$2(0, values, new FeedState$2(0, void 0, new FeedType$1(1)));
}

export function Res_Feed_emitManyAndReset(values) {
    return new Res$2(0, values, new FeedState$2(0, void 0, new FeedType$1(2)));
}

export function Res_Feed_emitManyAndResetFeedback(values) {
    return new Res$2(0, values, new FeedState$2(0, void 0, new FeedType$1(3)));
}

export function Res_Feed_emitManyAndResetDescendants(values, feedback) {
    return new Res$2(0, values, new FeedState$2(0, void 0, new FeedType$1(4, feedback)));
}

export function Res_Feed_emitManyAndStop(values) {
    return new Res$2(1, values);
}

export function Res_Feed_emit(value, feedback) {
    return Res_Feed_emitMany(singleton(value), feedback);
}

export function Res_Feed_emitAndKeepLast(value) {
    return Res_Feed_emitManyAndKeepLast(singleton(value));
}

export function Res_Feed_emitAndReset(value) {
    return Res_Feed_emitManyAndReset(singleton(value));
}

export function Res_Feed_emitAndResetFeedback(value) {
    return Res_Feed_emitManyAndResetFeedback(singleton(value));
}

export function Res_Feed_emitAndResetDescendants(value, feedback) {
    return Res_Feed_emitManyAndResetDescendants(singleton(value), feedback);
}

export function Res_Feed_emitAndStop(value) {
    return Res_Feed_emitManyAndStop(singleton(value));
}

export function Res_Feed_skip(feedback) {
    return Res_Feed_emitMany(empty(), feedback);
}

export function Res_Feed_skipAndKeepLast() {
    return Res_Feed_emitManyAndKeepLast(empty());
}

export function Res_Feed_skipAndReset() {
    return Res_Feed_emitManyAndReset(empty());
}

export function Res_Feed_skipAndResetFeedback() {
    return Res_Feed_emitManyAndResetFeedback(empty());
}

export function Res_Feed_skipAndResetDescendants(feedback) {
    return Res_Feed_emitManyAndResetDescendants(empty(), feedback);
}

export function Res_Feed_stop() {
    return Res_Feed_emitManyAndStop(empty());
}

export function Res_Feed_zero() {
    return Res_Feed_skipAndKeepLast();
}

export class Loop_Emit$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Emit"];
    }
}

export function Loop_Emit$1$reflection(gen0) {
    return union_type("LocSta.Loop.Emit`1", [gen0], Loop_Emit$1, () => [[["Item", gen0]]]);
}

export class Loop_EmitAndReset$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitAndReset"];
    }
}

export function Loop_EmitAndReset$1$reflection(gen0) {
    return union_type("LocSta.Loop.EmitAndReset`1", [gen0], Loop_EmitAndReset$1, () => [[["Item", gen0]]]);
}

export class Loop_EmitAndStop$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitAndStop"];
    }
}

export function Loop_EmitAndStop$1$reflection(gen0) {
    return union_type("LocSta.Loop.EmitAndStop`1", [gen0], Loop_EmitAndStop$1, () => [[["Item", gen0]]]);
}

export class Loop_EmitMany$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitMany"];
    }
}

export function Loop_EmitMany$1$reflection(gen0) {
    return union_type("LocSta.Loop.EmitMany`1", [gen0], Loop_EmitMany$1, () => [[["Item", list_type(gen0)]]]);
}

export class Loop_EmitManyAndReset$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitManyAndReset"];
    }
}

export function Loop_EmitManyAndReset$1$reflection(gen0) {
    return union_type("LocSta.Loop.EmitManyAndReset`1", [gen0], Loop_EmitManyAndReset$1, () => [[["Item", list_type(gen0)]]]);
}

export class Loop_EmitManyAndStop$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitManyAndStop"];
    }
}

export function Loop_EmitManyAndStop$1$reflection(gen0) {
    return union_type("LocSta.Loop.EmitManyAndStop`1", [gen0], Loop_EmitManyAndStop$1, () => [[["Item", list_type(gen0)]]]);
}

export class Loop_Skip extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Skip"];
    }
}

export function Loop_Skip$reflection() {
    return union_type("LocSta.Loop.Skip", [], Loop_Skip, () => [[]]);
}

export class Loop_SkipAndReset extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SkipAndReset"];
    }
}

export function Loop_SkipAndReset$reflection() {
    return union_type("LocSta.Loop.SkipAndReset", [], Loop_SkipAndReset, () => [[]]);
}

export class Loop_Stop extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Stop"];
    }
}

export function Loop_Stop$reflection() {
    return union_type("LocSta.Loop.Stop", [], Loop_Stop, () => [[]]);
}

export class Feed_Emit$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Emit"];
    }
}

export function Feed_Emit$2$reflection(gen0, gen1) {
    return union_type("LocSta.Feed.Emit`2", [gen0, gen1], Feed_Emit$2, () => [[["Item1", gen0], ["Item2", gen1]]]);
}

export class Feed_EmitAndKeepLast$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitAndKeepLast"];
    }
}

export function Feed_EmitAndKeepLast$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitAndKeepLast`1", [gen0], Feed_EmitAndKeepLast$1, () => [[["Item", gen0]]]);
}

export class Feed_EmitAndReset$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitAndReset"];
    }
}

export function Feed_EmitAndReset$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitAndReset`1", [gen0], Feed_EmitAndReset$1, () => [[["Item", gen0]]]);
}

export class Feed_EmitAndResetFeedback$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitAndResetFeedback"];
    }
}

export function Feed_EmitAndResetFeedback$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitAndResetFeedback`1", [gen0], Feed_EmitAndResetFeedback$1, () => [[["Item", gen0]]]);
}

export class Feed_EmitAndResetDescendants$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitAndResetDescendants"];
    }
}

export function Feed_EmitAndResetDescendants$2$reflection(gen0, gen1) {
    return union_type("LocSta.Feed.EmitAndResetDescendants`2", [gen0, gen1], Feed_EmitAndResetDescendants$2, () => [[["Item1", gen0], ["Item2", gen1]]]);
}

export class Feed_EmitAndStop$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitAndStop"];
    }
}

export function Feed_EmitAndStop$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitAndStop`1", [gen0], Feed_EmitAndStop$1, () => [[["Item", gen0]]]);
}

export class Feed_EmitMany$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitMany"];
    }
}

export function Feed_EmitMany$2$reflection(gen0, gen1) {
    return union_type("LocSta.Feed.EmitMany`2", [gen0, gen1], Feed_EmitMany$2, () => [[["Item1", list_type(gen0)], ["Item2", gen1]]]);
}

export class Feed_EmitManyAndKeepLast$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitManyAndKeepLast"];
    }
}

export function Feed_EmitManyAndKeepLast$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitManyAndKeepLast`1", [gen0], Feed_EmitManyAndKeepLast$1, () => [[["Item", list_type(gen0)]]]);
}

export class Feed_EmitManyAndReset$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitManyAndReset"];
    }
}

export function Feed_EmitManyAndReset$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitManyAndReset`1", [gen0], Feed_EmitManyAndReset$1, () => [[["Item", list_type(gen0)]]]);
}

export class Feed_EmitManyAndResetFeedback$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitManyAndResetFeedback"];
    }
}

export function Feed_EmitManyAndResetFeedback$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitManyAndResetFeedback`1", [gen0], Feed_EmitManyAndResetFeedback$1, () => [[["Item", list_type(gen0)]]]);
}

export class Feed_EmitManyAndResetDescendants$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitManyAndResetDescendants"];
    }
}

export function Feed_EmitManyAndResetDescendants$2$reflection(gen0, gen1) {
    return union_type("LocSta.Feed.EmitManyAndResetDescendants`2", [gen0, gen1], Feed_EmitManyAndResetDescendants$2, () => [[["Item1", list_type(gen0)], ["Item2", gen1]]]);
}

export class Feed_EmitManyAndStop$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["EmitManyAndStop"];
    }
}

export function Feed_EmitManyAndStop$1$reflection(gen0) {
    return union_type("LocSta.Feed.EmitManyAndStop`1", [gen0], Feed_EmitManyAndStop$1, () => [[["Item", list_type(gen0)]]]);
}

export class Feed_Skip$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Skip"];
    }
}

export function Feed_Skip$1$reflection(gen0) {
    return union_type("LocSta.Feed.Skip`1", [gen0], Feed_Skip$1, () => [[["Item", gen0]]]);
}

export class Feed_SkipAndKeepLast extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SkipAndKeepLast"];
    }
}

export function Feed_SkipAndKeepLast$reflection() {
    return union_type("LocSta.Feed.SkipAndKeepLast", [], Feed_SkipAndKeepLast, () => [[]]);
}

export class Feed_SkipAndReset extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SkipAndReset"];
    }
}

export function Feed_SkipAndReset$reflection() {
    return union_type("LocSta.Feed.SkipAndReset", [], Feed_SkipAndReset, () => [[]]);
}

export class Feed_SkipAndResetFeedback extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SkipAndResetFeedback"];
    }
}

export function Feed_SkipAndResetFeedback$reflection() {
    return union_type("LocSta.Feed.SkipAndResetFeedback", [], Feed_SkipAndResetFeedback, () => [[]]);
}

export class Feed_SkipAndResetDescendants$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SkipAndResetDescendants"];
    }
}

export function Feed_SkipAndResetDescendants$1$reflection(gen0) {
    return union_type("LocSta.Feed.SkipAndResetDescendants`1", [gen0], Feed_SkipAndResetDescendants$1, () => [[["Item", gen0]]]);
}

export class Feed_Stop extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Stop"];
    }
}

export function Feed_Stop$reflection() {
    return union_type("LocSta.Feed.Stop", [], Feed_Stop, () => [[]]);
}

export function Gen_run(gen) {
    const b = gen.fields[0];
    return b;
}

export function Gen_$007CLoopStateToOption$007C(defaultState, currState) {
    switch (currState.tag) {
        case 1: {
            return defaultState;
        }
        case 2: {
            return void 0;
        }
        default: {
            const s = currState.fields[0];
            return some(s);
        }
    }
}

export function Gen_ofRepeatingValues(values) {
    return new Gen$2(0, (_arg1) => (new Res$2(0, values, new LoopState$1(1))));
}

export function Gen_ofRepeatingValue(value) {
    return Gen_ofRepeatingValues(singleton(value));
}

export function Gen_ofOneTimeValues(values) {
    return new Gen$2(0, (_arg1) => (new Res$2(1, values)));
}

export function Gen_ofOneTimeValue(value) {
    return Gen_ofOneTimeValues(singleton(value));
}

export function Gen_ofSeqOneByOne(s) {
    return new Gen$2(0, (enumerator) => {
        const enumerator_1 = defaultArgWith(enumerator, () => getEnumerator(s));
        return enumerator_1["System.Collections.IEnumerator.MoveNext"]() ? Res_Loop_emit(enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"](), enumerator_1) : Res_Loop_stop();
    });
}

export function Gen_ofListOneByOne(list) {
    return new Gen$2(0, (l) => {
        const l_1 = defaultArg(l, list);
        if (isEmpty(l_1)) {
            return Res_Loop_stop();
        }
        else {
            const xs = tail(l_1);
            const x = head(l_1);
            return Res_Loop_emit(x, xs);
        }
    });
}

export function Gen_ofListAllAtOnce(list) {
    return new Gen$2(0, (_arg1) => {
        if (isEmpty(list)) {
            return Res_Loop_stop();
        }
        else {
            const l = list;
            return Res_Loop_emitManyAndKeepLast(l);
        }
    });
}

export class Gen_CombineInfo$2 extends Record {
    constructor(astate, bstate) {
        super();
        this.astate = astate;
        this.bstate = bstate;
    }
}

export function Gen_CombineInfo$2$reflection(gen0, gen1) {
    return record_type("LocSta.Gen.CombineInfo`2", [gen0, gen1], Gen_CombineInfo$2, () => [["astate", option_type(gen0)], ["bstate", option_type(gen1)]]);
}

export class Gen_Evaluable$1 {
    constructor(f) {
        this.f = f;
    }
}

export function Gen_Evaluable$1$reflection(gen0) {
    return class_type("LocSta.Gen.Evaluable`1", [gen0], Gen_Evaluable$1);
}

export function Gen_Evaluable$1_$ctor_9CB17FF(f) {
    return new Gen_Evaluable$1(f);
}

export function Gen_Evaluable$1__Evaluate(_) {
    return _.f();
}

export function Gen_toEvaluable(g) {
    const f = Gen_run(g);
    let state = void 0;
    let resume = true;
    let remainingValues = empty();
    const getNext = () => {
        const matchValue = [remainingValues, resume];
        if (isEmpty(matchValue[0])) {
            if (matchValue[1]) {
                const matchValue_1 = f(state);
                if (matchValue_1.tag === 1) {
                    const values_1 = matchValue_1.fields[0];
                    resume = false;
                    remainingValues = values_1;
                    return getNext();
                }
                else {
                    const values = matchValue_1.fields[0];
                    const fstate = Gen_$007CLoopStateToOption$007C(state, matchValue_1.fields[1]);
                    state = fstate;
                    remainingValues = values;
                    return getNext();
                }
            }
            else {
                return void 0;
            }
        }
        else {
            const xs = tail(matchValue[0]);
            const x = head(matchValue[0]);
            remainingValues = xs;
            return some(x);
        }
    };
    return Gen_Evaluable$1_$ctor_9CB17FF(getNext);
}

export function Gen_toSeq(g) {
    const f = Gen_run(g);
    let state = void 0;
    let resume = true;
    return delay(() => enumerateWhile(() => resume, delay(() => {
        const matchValue = f(state);
        if (matchValue.tag === 1) {
            const values_1 = matchValue.fields[0];
            resume = false;
            return values_1;
        }
        else {
            const values = matchValue.fields[0];
            const fstate = Gen_$007CLoopStateToOption$007C(state, matchValue.fields[1]);
            state = fstate;
            return values;
        }
    })));
}

export function Gen_toList(gen) {
    return toList(Gen_toSeq(gen));
}

export function Gen_toListn(numOfElements, gen) {
    return toList(truncate(numOfElements, Gen_toSeq(gen)));
}

export function Gen_toFx(gen, unitVar0) {
    return gen;
}

export class Gen_BaseBuilder {
    constructor() {
    }
}

export function Gen_BaseBuilder$reflection() {
    return class_type("LocSta.Gen.BaseBuilder", void 0, Gen_BaseBuilder);
}

export function Gen_BaseBuilder_$ctor() {
    return new Gen_BaseBuilder();
}

export function Gen_BaseBuilder__ReturnFrom_1505(_, x) {
    return x;
}

export function Gen_BaseBuilder__YieldFrom_Z43573BF7(_, x) {
    return Gen_ofListAllAtOnce(x);
}

export function Gen_BaseBuilder__Delay_1505(_, delayed) {
    return delayed;
}

export function Gen_BaseBuilder__For_Z48B64FBB(_, list, body) {
    let state, resume, enumerator;
    return Gen_ofListAllAtOnce(toList((state = (void 0), (resume = true, (enumerator = getEnumerator(list), delay(() => enumerateWhile(() => (resume ? enumerator["System.Collections.IEnumerator.MoveNext"]() : false), delay(() => {
        const matchValue = Gen_run(body(enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]()))(state);
        if (matchValue.tag === 1) {
            const values_1 = matchValue.fields[0];
            resume = false;
            return values_1;
        }
        else {
            const values = matchValue.fields[0];
            const fstate = Gen_$007CLoopStateToOption$007C(state, matchValue.fields[1]);
            state = fstate;
            return values;
        }
    }))))))));
}

export class Gen_LoopBuilder extends Gen_BaseBuilder {
    constructor() {
        super();
    }
}

export function Gen_LoopBuilder$reflection() {
    return class_type("LocSta.Gen.LoopBuilder", void 0, Gen_LoopBuilder, Gen_BaseBuilder$reflection());
}

export function Gen_LoopBuilder_$ctor() {
    return new Gen_LoopBuilder();
}

export function Gen_LoopBuilder__Combine_463FDD0A(_, x, delayed) {
    return new Gen$2(0, (state) => {
        const state_1 = defaultArg(state, new Gen_CombineInfo$2(void 0, void 0));
        const ares = Gen_run(x)(state_1.astate);
        if (ares.tag === 1) {
            const avalues_1 = ares.fields[0];
            return new Res$2(1, avalues_1);
        }
        else {
            const avalues = ares.fields[0];
            const astate = Gen_$007CLoopStateToOption$007C(state_1.astate, ares.fields[1]);
            const bres = Gen_run(delayed())(state_1.bstate);
            if (bres.tag === 1) {
                const bvalues_1 = bres.fields[0];
                return Res_Loop_emitManyAndStop(append(avalues, bvalues_1));
            }
            else {
                const bvalues = bres.fields[0];
                const bstate = Gen_$007CLoopStateToOption$007C(state_1.bstate, bres.fields[1]);
                return Res_Loop_emitMany(append(avalues, bvalues), new Gen_CombineInfo$2(astate, bstate));
            }
        }
    });
}

export function Gen_LoopBuilder__Zero(_) {
    const res = Res_Loop_zero();
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Yield_1505(_, value) {
    const res = Res_Loop_emitAndKeepLast(value);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_Z531701A5(_, _arg1) {
    const value = _arg1.fields[0];
    const res = Res_Loop_emitAndKeepLast(value);
    return new Gen$2(0, (_arg1_1) => res);
}

export function Gen_LoopBuilder__Return_Z3449CE5B(_, _arg2) {
    const value = _arg2.fields[0];
    const res = Res_Loop_emitAndReset(value);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_ZAE17398(_, _arg3) {
    const value = _arg3.fields[0];
    const res = Res_Loop_emitAndStop(value);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_Z7BD98500(_, _arg4) {
    const values = _arg4.fields[0];
    const res = Res_Loop_emitManyAndKeepLast(values);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_Z48160702(_, _arg5) {
    const values = _arg5.fields[0];
    const res = Res_Loop_emitManyAndReset(values);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_26201B93(_, _arg6) {
    const values = _arg6.fields[0];
    const res = Res_Loop_emitManyAndStop(values);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_2CC912DE(_, _arg7) {
    const res = Res_Loop_skipAndKeepLast();
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_3CCCF4A0(_, _arg8) {
    const res = Res_Loop_skipAndReset();
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_LoopBuilder__Return_2CC951C7(_, _arg9) {
    const res = Res_Loop_stop();
    return new Gen$2(0, (_arg1) => res);
}

export class Gen_FeedBuilder extends Gen_BaseBuilder {
    constructor() {
        super();
    }
}

export function Gen_FeedBuilder$reflection() {
    return class_type("LocSta.Gen.FeedBuilder", void 0, Gen_FeedBuilder, Gen_BaseBuilder$reflection());
}

export function Gen_FeedBuilder_$ctor() {
    return new Gen_FeedBuilder();
}

export function Gen_FeedBuilder__Zero(_) {
    const res = Res_Feed_zero();
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Yield_2A0A0(_, value, feedback) {
    const res = Res_Feed_emit(value, feedback);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z2DCF3B94(_, _arg1) {
    const value = _arg1.fields[0];
    const feedback = _arg1.fields[1];
    const res = Res_Feed_emit(value, feedback);
    return new Gen$2(0, (_arg1_1) => res);
}

export function Gen_FeedBuilder__Return_16BB62BF(_, _arg2) {
    const value = _arg2.fields[0];
    const res = Res_Feed_emitAndKeepLast(value);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z73E685(_, _arg3) {
    const value = _arg3.fields[0];
    const res = Res_Feed_emitAndReset(value);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_52CCB8D2(_, _arg4) {
    const value = _arg4.fields[0];
    const res = Res_Feed_emitAndResetFeedback(value);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_7C29FB64(_, _arg5) {
    const value = _arg5.fields[0];
    const feedback = _arg5.fields[1];
    const res = Res_Feed_emitAndResetDescendants(value, feedback);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z5BEBAC8A(_, _arg6) {
    const value = _arg6.fields[0];
    const res = Res_Feed_emitAndStop(value);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z6686A729(_, _arg7) {
    const values = _arg7.fields[0];
    const feedback = _arg7.fields[1];
    const res = Res_Feed_emitMany(values, feedback);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_12885A84(_, _arg8) {
    const values = _arg8.fields[0];
    const res = Res_Feed_emitManyAndKeepLast(values);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z2ED10DE0(_, _arg9) {
    const values = _arg9.fields[0];
    const res = Res_Feed_emitManyAndReset(values);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z5D1E60B7(_, _arg10) {
    const values = _arg10.fields[0];
    const res = Res_Feed_emitManyAndResetFeedback(values);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z57703541(_, _arg11) {
    const values = _arg11.fields[0];
    const feedback = _arg11.fields[1];
    const res = Res_Feed_emitManyAndResetDescendants(values, feedback);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_3C05DB8D(_, _arg12) {
    const values = _arg12.fields[0];
    const res = Res_Feed_emitManyAndStop(values);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z46B6E02F(_, _arg13) {
    const feedback = _arg13.fields[0];
    const res = Res_Feed_skip(feedback);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_1F197D3A(_, _arg14) {
    const res = Res_Feed_skipAndKeepLast();
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_435D107E(_, _arg15) {
    const res = Res_Feed_skipAndReset();
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z2C47D729(_, _arg16) {
    const res = Res_Feed_skipAndResetFeedback();
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_18629FD9(_, _arg17) {
    const feedback = _arg17.fields[0];
    const res = Res_Feed_skipAndResetDescendants(feedback);
    return new Gen$2(0, (_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z3B1223E7(_, _arg18) {
    const res = Res_Feed_stop();
    return new Gen$2(0, (_arg1) => res);
}

export const Gen_loop = Gen_LoopBuilder_$ctor();

export const Gen_feed = Gen_FeedBuilder_$ctor();

export function Gen_apply(xGen, fGen) {
    const builder$0040 = Gen_loop;
    return Gen_BaseBuilder__Delay_1505(builder$0040, () => {
        const evalk_2 = (mval_3, mstate_3, mleftovers_3, lastKState_3, isStopped_3) => {
            let l$0027, evalk, buildSkip, evalk_1, m_2;
            const matchValue_1 = Gen_run((l$0027 = mval_3, (evalk = ((mval, mstate, mleftovers, lastKState, isStopped) => {
                let f$0027, result;
                const matchValue = Gen_run((f$0027 = mval, (result = f$0027(l$0027), Gen_LoopBuilder__Yield_1505(builder$0040, result))))(lastKState);
                if (matchValue.tag === 1) {
                    const kvalues_1 = matchValue.fields[0];
                    return new Res$2(1, kvalues_1);
                }
                else {
                    const kvalues = matchValue.fields[0];
                    const kstate = matchValue.fields[1];
                    const newState = (kstate_1) => (new BindState$3(mstate, kstate_1, mleftovers, isStopped));
                    switch (kstate.tag) {
                        case 1: {
                            return new Res$2(0, kvalues, new LoopState$1(0, newState(lastKState)));
                        }
                        case 2: {
                            return new Res$2(0, kvalues, new LoopState$1(2));
                        }
                        default: {
                            const kstate_2 = kstate.fields[0];
                            return new Res$2(0, kvalues, new LoopState$1(0, newState(some(kstate_2))));
                        }
                    }
                }
            }), (buildSkip = ((state) => (new LoopState$1(0, state))), (evalk_1 = evalk, (m_2 = fGen, new Gen$2(0, (state_1) => {
                let kstate_3, lastMState_2, isStopped_2, lastKState_2, lastMState_1, x, xs;
                const evalmres = (mres, lastMState, lastKState_1, isStopped_1) => {
                    if (mres.tag === 1) {
                        if (isEmpty(mres.fields[0])) {
                            return new Res$2(1, empty());
                        }
                        else {
                            const mleftovers_2 = tail(mres.fields[0]);
                            const mval_2 = head(mres.fields[0]);
                            return evalk_1(mval_2, lastMState, mleftovers_2, lastKState_1, isStopped_1);
                        }
                    }
                    else if (isEmpty(mres.fields[0])) {
                        const activePatternResult854 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mstate_2 = activePatternResult854;
                        const state_2 = new BindState$3(mstate_2, lastKState_1, empty(), isStopped_1);
                        return new Res$2(0, empty(), buildSkip(state_2));
                    }
                    else {
                        const activePatternResult853 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mleftovers_1 = tail(mres.fields[0]);
                        const mstate_1 = activePatternResult853;
                        const mval_1 = head(mres.fields[0]);
                        return evalk_1(mval_1, mstate_1, mleftovers_1, lastKState_1, isStopped_1);
                    }
                };
                return (state_1 == null) ? evalmres(Gen_run(m_2)(void 0), void 0, void 0, false) : (isEmpty(state_1.mleftovers) ? (state_1.isStopped ? (new Res$2(1, empty())) : ((kstate_3 = state_1.kstate, (lastMState_2 = state_1.mstate, evalmres(Gen_run(m_2)(lastMState_2), lastMState_2, kstate_3, false))))) : ((isStopped_2 = state_1.isStopped, (lastKState_2 = state_1.kstate, (lastMState_1 = state_1.mstate, (x = head(state_1.mleftovers), (xs = tail(state_1.mleftovers), evalk_1(x, lastMState_1, xs, lastKState_2, isStopped_2))))))));
            })))))))(lastKState_3);
            if (matchValue_1.tag === 1) {
                const kvalues_3 = matchValue_1.fields[0];
                return new Res$2(1, kvalues_3);
            }
            else {
                const kvalues_2 = matchValue_1.fields[0];
                const kstate_4 = matchValue_1.fields[1];
                const newState_1 = (kstate_5) => (new BindState$3(mstate_3, kstate_5, mleftovers_3, isStopped_3));
                switch (kstate_4.tag) {
                    case 1: {
                        return new Res$2(0, kvalues_2, new LoopState$1(0, newState_1(lastKState_3)));
                    }
                    case 2: {
                        return new Res$2(0, kvalues_2, new LoopState$1(2));
                    }
                    default: {
                        const kstate_6 = kstate_4.fields[0];
                        return new Res$2(0, kvalues_2, new LoopState$1(0, newState_1(kstate_6)));
                    }
                }
            }
        };
        const buildSkip_2 = (state_3) => (new LoopState$1(0, state_3));
        const evalk_3 = evalk_2;
        const m_5 = xGen;
        return new Gen$2(0, (state_4) => {
            let kstate_7, lastMState_5, isStopped_5, lastKState_5, lastMState_4, x_1, xs_1;
            const evalmres_1 = (mres_1, lastMState_3, lastKState_4, isStopped_4) => {
                if (mres_1.tag === 1) {
                    if (isEmpty(mres_1.fields[0])) {
                        return new Res$2(1, empty());
                    }
                    else {
                        const mleftovers_5 = tail(mres_1.fields[0]);
                        const mval_5 = head(mres_1.fields[0]);
                        return evalk_3(mval_5, lastMState_3, mleftovers_5, lastKState_4, isStopped_4);
                    }
                }
                else if (isEmpty(mres_1.fields[0])) {
                    const activePatternResult854_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mstate_5 = activePatternResult854_1;
                    const state_5 = new BindState$3(mstate_5, lastKState_4, empty(), isStopped_4);
                    return new Res$2(0, empty(), buildSkip_2(state_5));
                }
                else {
                    const activePatternResult853_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mleftovers_4 = tail(mres_1.fields[0]);
                    const mstate_4 = activePatternResult853_1;
                    const mval_4 = head(mres_1.fields[0]);
                    return evalk_3(mval_4, mstate_4, mleftovers_4, lastKState_4, isStopped_4);
                }
            };
            return (state_4 == null) ? evalmres_1(Gen_run(m_5)(void 0), void 0, void 0, false) : (isEmpty(state_4.mleftovers) ? (state_4.isStopped ? (new Res$2(1, empty())) : ((kstate_7 = state_4.kstate, (lastMState_5 = state_4.mstate, evalmres_1(Gen_run(m_5)(lastMState_5), lastMState_5, kstate_7, false))))) : ((isStopped_5 = state_4.isStopped, (lastKState_5 = state_4.kstate, (lastMState_4 = state_4.mstate, (x_1 = head(state_4.mleftovers), (xs_1 = tail(state_4.mleftovers), evalk_3(x_1, lastMState_4, xs_1, lastKState_5, isStopped_5))))))));
        });
    })();
}

export function Gen_withState(inputGen) {
    return new Gen$2(0, (state) => {
        const mapValues = (values, state_1) => toList(delay(() => map((v_1) => [v_1, state_1], values)));
        const matchValue = Gen_run(inputGen)(state);
        if (matchValue.tag === 1) {
            const values_2 = matchValue.fields[0];
            return Res_Loop_emitManyAndStop(mapValues(values_2, void 0));
        }
        else {
            const values_1 = matchValue.fields[0];
            const state_2 = matchValue.fields[1];
            return new Res$2(0, mapValues(values_1, state_2), state_2);
        }
    });
}

export class Gen_OnStopThenState$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["RunInput", "UseDefault"];
    }
}

export function Gen_OnStopThenState$1$reflection(gen0) {
    return union_type("LocSta.Gen.OnStopThenState`1", [gen0], Gen_OnStopThenState$1, () => [[["Item", option_type(gen0)]], []]);
}

export function Gen_resetWhenStop(inputGen) {
    return new Gen$2(0, (state) => {
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

export function Gen_accumulate(value) {
    const builder$0040 = Gen_feed;
    return Gen_BaseBuilder__Delay_1505(builder$0040, () => {
        const m_1 = new Init$1(0, empty());
        return new Gen$2(0, (state) => {
            let feedback_4, kstate_3, kstate_2;
            const getInitial = () => {
                if (m_1.tag === 1) {
                    const f_1 = m_1.fields[0];
                    return f_1();
                }
                else {
                    const m_2 = m_1.fields[0];
                    return m_2;
                }
            };
            const evalk = (lastFeed, lastKState) => {
                let elements, newElements, tupledArg;
                const matchValue = Gen_run((elements = lastFeed, (newElements = append(elements, singleton(value)), (tupledArg = [newElements, newElements], Gen_FeedBuilder__Yield_2A0A0(builder$0040, tupledArg[0], tupledArg[1])))))(lastKState);
                if (matchValue.tag === 1) {
                    const kvalues_1 = matchValue.fields[0];
                    return new Res$2(1, kvalues_1);
                }
                else {
                    const kvalues = matchValue.fields[0];
                    const kstate = matchValue.fields[1].fields[0];
                    const feedback = matchValue.fields[1].fields[1];
                    let patternInput;
                    switch (feedback.tag) {
                        case 1: {
                            patternInput = [lastFeed, kstate];
                            break;
                        }
                        case 2: {
                            patternInput = [void 0, void 0];
                            break;
                        }
                        case 3: {
                            patternInput = [void 0, kstate];
                            break;
                        }
                        case 4: {
                            const feedback_2 = feedback.fields[0];
                            patternInput = [feedback_2, void 0];
                            break;
                        }
                        default: {
                            const feedback_1 = feedback.fields[0];
                            patternInput = [feedback_1, kstate];
                        }
                    }
                    const kstate_1 = patternInput[1];
                    const feedback_3 = patternInput[0];
                    const state_1 = new BindState$3(feedback_3, kstate_1, empty(), false);
                    return new Res$2(0, kvalues, new LoopState$1(0, state_1));
                }
            };
            return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = state.mstate, (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
        });
    })();
}

export function Gen_accumulateOnePart(partLength, value) {
    const builder$0040 = Gen_loop;
    return Gen_BaseBuilder__Delay_1505(builder$0040, () => {
        const evalk_3 = (mval_3, mstate_3, mleftovers_3, lastKState_4, isStopped_3) => {
            let c, evalk_1, buildSkip, evalk_2, m_5;
            const matchValue_2 = Gen_run((c = (mval_3 | 0), (evalk_1 = ((mval, mstate, mleftovers, lastKState_1, isStopped) => {
                let acc;
                const matchValue_1 = Gen_run((acc = mval, (c === (partLength - 1)) ? Gen_LoopBuilder__Yield_1505(builder$0040, acc) : ((c === partLength) ? Gen_LoopBuilder__Return_2CC951C7(builder$0040, new Loop_Stop(0)) : Gen_LoopBuilder__Zero(builder$0040))))(lastKState_1);
                if (matchValue_1.tag === 1) {
                    const kvalues_3 = matchValue_1.fields[0];
                    return new Res$2(1, kvalues_3);
                }
                else {
                    const kvalues_2 = matchValue_1.fields[0];
                    const kstate_4 = matchValue_1.fields[1];
                    const newState = (kstate_5) => (new BindState$3(mstate, kstate_5, mleftovers, isStopped));
                    switch (kstate_4.tag) {
                        case 1: {
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(lastKState_1)));
                        }
                        case 2: {
                            return new Res$2(0, kvalues_2, new LoopState$1(2));
                        }
                        default: {
                            const kstate_6 = kstate_4.fields[0];
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(some(kstate_6))));
                        }
                    }
                }
            }), (buildSkip = ((state_2) => (new LoopState$1(0, state_2))), (evalk_2 = evalk_1, (m_5 = Gen_accumulate(value), new Gen$2(0, (state_3) => {
                let kstate_7, lastMState_2, isStopped_2, lastKState_3, lastMState_1, x, xs;
                const evalmres = (mres, lastMState, lastKState_2, isStopped_1) => {
                    if (mres.tag === 1) {
                        if (isEmpty(mres.fields[0])) {
                            return new Res$2(1, empty());
                        }
                        else {
                            const mleftovers_2 = tail(mres.fields[0]);
                            const mval_2 = head(mres.fields[0]);
                            return evalk_2(mval_2, lastMState, mleftovers_2, lastKState_2, isStopped_1);
                        }
                    }
                    else if (isEmpty(mres.fields[0])) {
                        const activePatternResult854 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mstate_2 = activePatternResult854;
                        const state_4 = new BindState$3(mstate_2, lastKState_2, empty(), isStopped_1);
                        return new Res$2(0, empty(), buildSkip(state_4));
                    }
                    else {
                        const activePatternResult853 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mleftovers_1 = tail(mres.fields[0]);
                        const mstate_1 = activePatternResult853;
                        const mval_1 = head(mres.fields[0]);
                        return evalk_2(mval_1, mstate_1, mleftovers_1, lastKState_2, isStopped_1);
                    }
                };
                return (state_3 == null) ? evalmres(Gen_run(m_5)(void 0), void 0, void 0, false) : (isEmpty(state_3.mleftovers) ? (state_3.isStopped ? (new Res$2(1, empty())) : ((kstate_7 = state_3.kstate, (lastMState_2 = state_3.mstate, evalmres(Gen_run(m_5)(lastMState_2), lastMState_2, kstate_7, false))))) : ((isStopped_2 = state_3.isStopped, (lastKState_3 = state_3.kstate, (lastMState_1 = state_3.mstate, (x = head(state_3.mleftovers), (xs = tail(state_3.mleftovers), evalk_2(x, lastMState_1, xs, lastKState_3, isStopped_2))))))));
            })))))))(lastKState_4);
            if (matchValue_2.tag === 1) {
                const kvalues_5 = matchValue_2.fields[0];
                return new Res$2(1, kvalues_5);
            }
            else {
                const kvalues_4 = matchValue_2.fields[0];
                const kstate_8 = matchValue_2.fields[1];
                const newState_1 = (kstate_9) => (new BindState$3(mstate_3, kstate_9, mleftovers_3, isStopped_3));
                switch (kstate_8.tag) {
                    case 1: {
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(lastKState_4)));
                    }
                    case 2: {
                        return new Res$2(0, kvalues_4, new LoopState$1(2));
                    }
                    default: {
                        const kstate_10 = kstate_8.fields[0];
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(kstate_10)));
                    }
                }
            }
        };
        const buildSkip_2 = (state_5) => (new LoopState$1(0, state_5));
        const evalk_4 = evalk_3;
        let m_8;
        const builder$0040_1 = Gen_feed;
        m_8 = Gen_BaseBuilder__Delay_1505(builder$0040_1, () => {
            const m_1 = new Init$1(0, 0);
            return new Gen$2(0, (state) => {
                let feedback_4, kstate_3, kstate_2;
                const getInitial = () => {
                    if (m_1.tag === 1) {
                        const f_1 = m_1.fields[0];
                        return f_1() | 0;
                    }
                    else {
                        const m_2 = m_1.fields[0];
                        return m_2 | 0;
                    }
                };
                const evalk = (lastFeed, lastKState) => {
                    let curr, tupledArg;
                    const matchValue = Gen_run((curr = (lastFeed | 0), (tupledArg = [curr, curr + 1], Gen_FeedBuilder__Yield_2A0A0(builder$0040_1, tupledArg[0], tupledArg[1]))))(lastKState);
                    if (matchValue.tag === 1) {
                        const kvalues_1 = matchValue.fields[0];
                        return new Res$2(1, kvalues_1);
                    }
                    else {
                        const kvalues = matchValue.fields[0];
                        const kstate = matchValue.fields[1].fields[0];
                        const feedback = matchValue.fields[1].fields[1];
                        let patternInput;
                        switch (feedback.tag) {
                            case 1: {
                                patternInput = [lastFeed, kstate];
                                break;
                            }
                            case 2: {
                                patternInput = [void 0, void 0];
                                break;
                            }
                            case 3: {
                                patternInput = [void 0, kstate];
                                break;
                            }
                            case 4: {
                                const feedback_2 = feedback.fields[0];
                                patternInput = [feedback_2, void 0];
                                break;
                            }
                            default: {
                                const feedback_1 = feedback.fields[0];
                                patternInput = [feedback_1, kstate];
                            }
                        }
                        const kstate_1 = patternInput[1];
                        const feedback_3 = patternInput[0];
                        const state_1 = new BindState$3(feedback_3, kstate_1, empty(), false);
                        return new Res$2(0, kvalues, new LoopState$1(0, state_1));
                    }
                };
                return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = (state.mstate | 0), (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
            });
        })();
        return new Gen$2(0, (state_6) => {
            let kstate_11, lastMState_5, isStopped_5, lastKState_6, lastMState_4, x_1, xs_1;
            const evalmres_1 = (mres_1, lastMState_3, lastKState_5, isStopped_4) => {
                if (mres_1.tag === 1) {
                    if (isEmpty(mres_1.fields[0])) {
                        return new Res$2(1, empty());
                    }
                    else {
                        const mleftovers_5 = tail(mres_1.fields[0]);
                        const mval_5 = head(mres_1.fields[0]) | 0;
                        return evalk_4(mval_5, lastMState_3, mleftovers_5, lastKState_5, isStopped_4);
                    }
                }
                else if (isEmpty(mres_1.fields[0])) {
                    const activePatternResult854_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mstate_5 = activePatternResult854_1;
                    const state_7 = new BindState$3(mstate_5, lastKState_5, empty(), isStopped_4);
                    return new Res$2(0, empty(), buildSkip_2(state_7));
                }
                else {
                    const activePatternResult853_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mleftovers_4 = tail(mres_1.fields[0]);
                    const mstate_4 = activePatternResult853_1;
                    const mval_4 = head(mres_1.fields[0]) | 0;
                    return evalk_4(mval_4, mstate_4, mleftovers_4, lastKState_5, isStopped_4);
                }
            };
            return (state_6 == null) ? evalmres_1(Gen_run(m_8)(void 0), void 0, void 0, false) : (isEmpty(state_6.mleftovers) ? (state_6.isStopped ? (new Res$2(1, empty())) : ((kstate_11 = state_6.kstate, (lastMState_5 = state_6.mstate, evalmres_1(Gen_run(m_8)(lastMState_5), lastMState_5, kstate_11, false))))) : ((isStopped_5 = state_6.isStopped, (lastKState_6 = state_6.kstate, (lastMState_4 = state_6.mstate, (x_1 = (head(state_6.mleftovers) | 0), (xs_1 = tail(state_6.mleftovers), evalk_4(x_1, lastMState_4, xs_1, lastKState_6, isStopped_5))))))));
        });
    })();
}

export function Gen_accumulateManyParts(count, currentValue) {
    return Gen_resetWhenStop(Gen_accumulateOnePart(count, currentValue));
}

export function Gen_fork(inputGen) {
    const builder$0040 = Gen_feed;
    return Gen_BaseBuilder__Delay_1505(builder$0040, () => {
        const m_1 = new Init$1(0, empty());
        return new Gen$2(0, (state) => {
            let feedback_4, kstate_3, kstate_2;
            const getInitial = () => {
                if (m_1.tag === 1) {
                    const f_1 = m_1.fields[0];
                    return f_1();
                }
                else {
                    const m_2 = m_1.fields[0];
                    return m_2;
                }
            };
            const evalk = (lastFeed, lastKState) => {
                let runningStates, inputGen_1, resultValues, newForkStates;
                const matchValue_1 = Gen_run((runningStates = lastFeed, (inputGen_1 = Gen_run(inputGen), (resultValues = empty(), (newForkStates = toList(delay(() => collect((forkState) => {
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
                }, cons(void 0, runningStates)))), Gen_FeedBuilder__Return_Z6686A729(builder$0040, new Feed_EmitMany$2(0, resultValues, newForkStates)))))))(lastKState);
                if (matchValue_1.tag === 1) {
                    const kvalues_1 = matchValue_1.fields[0];
                    return new Res$2(1, kvalues_1);
                }
                else {
                    const kvalues = matchValue_1.fields[0];
                    const kstate = matchValue_1.fields[1].fields[0];
                    const feedback = matchValue_1.fields[1].fields[1];
                    let patternInput;
                    switch (feedback.tag) {
                        case 1: {
                            patternInput = [lastFeed, kstate];
                            break;
                        }
                        case 2: {
                            patternInput = [void 0, void 0];
                            break;
                        }
                        case 3: {
                            patternInput = [void 0, kstate];
                            break;
                        }
                        case 4: {
                            const feedback_2 = feedback.fields[0];
                            patternInput = [feedback_2, void 0];
                            break;
                        }
                        default: {
                            const feedback_1 = feedback.fields[0];
                            patternInput = [feedback_1, kstate];
                        }
                    }
                    const kstate_1 = patternInput[1];
                    const feedback_3 = patternInput[0];
                    const state_1 = new BindState$3(feedback_3, kstate_1, empty(), false);
                    return new Res$2(0, kvalues, new LoopState$1(0, state_1));
                }
            };
            return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = state.mstate, (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
        });
    })();
}

export function Gen_head(g) {
    return exactlyOne(Gen_toListn(1, g));
}

export function Gen_skip(n, g) {
    const builder$0040 = Gen_loop;
    return Gen_BaseBuilder__Delay_1505(builder$0040, () => {
        const evalk_3 = (mval_3, mstate_3, mleftovers_3, lastKState_4, isStopped_3) => {
            let v, evalk_1, buildSkip, evalk_2, m_5, builder$0040_1;
            const matchValue_2 = Gen_run((v = mval_3, (evalk_1 = ((mval, mstate, mleftovers, lastKState_1, isStopped) => {
                let c;
                const matchValue_1 = Gen_run((c = (mval | 0), (c >= n) ? Gen_LoopBuilder__Yield_1505(builder$0040, v) : Gen_LoopBuilder__Zero(builder$0040)))(lastKState_1);
                if (matchValue_1.tag === 1) {
                    const kvalues_3 = matchValue_1.fields[0];
                    return new Res$2(1, kvalues_3);
                }
                else {
                    const kvalues_2 = matchValue_1.fields[0];
                    const kstate_4 = matchValue_1.fields[1];
                    const newState = (kstate_5) => (new BindState$3(mstate, kstate_5, mleftovers, isStopped));
                    switch (kstate_4.tag) {
                        case 1: {
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(lastKState_1)));
                        }
                        case 2: {
                            return new Res$2(0, kvalues_2, new LoopState$1(2));
                        }
                        default: {
                            const kstate_6 = kstate_4.fields[0];
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(some(kstate_6))));
                        }
                    }
                }
            }), (buildSkip = ((state_2) => (new LoopState$1(0, state_2))), (evalk_2 = evalk_1, (m_5 = ((builder$0040_1 = Gen_feed, Gen_BaseBuilder__Delay_1505(builder$0040_1, () => {
                const m_1 = new Init$1(0, 0);
                return new Gen$2(0, (state) => {
                    let feedback_4, kstate_3, kstate_2;
                    const getInitial = () => {
                        if (m_1.tag === 1) {
                            const f_1 = m_1.fields[0];
                            return f_1() | 0;
                        }
                        else {
                            const m_2 = m_1.fields[0];
                            return m_2 | 0;
                        }
                    };
                    const evalk = (lastFeed, lastKState) => {
                        let curr, tupledArg;
                        const matchValue = Gen_run((curr = (lastFeed | 0), (tupledArg = [curr, curr + 1], Gen_FeedBuilder__Yield_2A0A0(builder$0040_1, tupledArg[0], tupledArg[1]))))(lastKState);
                        if (matchValue.tag === 1) {
                            const kvalues_1 = matchValue.fields[0];
                            return new Res$2(1, kvalues_1);
                        }
                        else {
                            const kvalues = matchValue.fields[0];
                            const kstate = matchValue.fields[1].fields[0];
                            const feedback = matchValue.fields[1].fields[1];
                            let patternInput;
                            switch (feedback.tag) {
                                case 1: {
                                    patternInput = [lastFeed, kstate];
                                    break;
                                }
                                case 2: {
                                    patternInput = [void 0, void 0];
                                    break;
                                }
                                case 3: {
                                    patternInput = [void 0, kstate];
                                    break;
                                }
                                case 4: {
                                    const feedback_2 = feedback.fields[0];
                                    patternInput = [feedback_2, void 0];
                                    break;
                                }
                                default: {
                                    const feedback_1 = feedback.fields[0];
                                    patternInput = [feedback_1, kstate];
                                }
                            }
                            const kstate_1 = patternInput[1];
                            const feedback_3 = patternInput[0];
                            const state_1 = new BindState$3(feedback_3, kstate_1, empty(), false);
                            return new Res$2(0, kvalues, new LoopState$1(0, state_1));
                        }
                    };
                    return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = (state.mstate | 0), (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
                });
            })())), new Gen$2(0, (state_3) => {
                let kstate_7, lastMState_2, isStopped_2, lastKState_3, lastMState_1, x, xs;
                const evalmres = (mres, lastMState, lastKState_2, isStopped_1) => {
                    if (mres.tag === 1) {
                        if (isEmpty(mres.fields[0])) {
                            return new Res$2(1, empty());
                        }
                        else {
                            const mleftovers_2 = tail(mres.fields[0]);
                            const mval_2 = head(mres.fields[0]) | 0;
                            return evalk_2(mval_2, lastMState, mleftovers_2, lastKState_2, isStopped_1);
                        }
                    }
                    else if (isEmpty(mres.fields[0])) {
                        const activePatternResult854 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mstate_2 = activePatternResult854;
                        const state_4 = new BindState$3(mstate_2, lastKState_2, empty(), isStopped_1);
                        return new Res$2(0, empty(), buildSkip(state_4));
                    }
                    else {
                        const activePatternResult853 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mleftovers_1 = tail(mres.fields[0]);
                        const mstate_1 = activePatternResult853;
                        const mval_1 = head(mres.fields[0]) | 0;
                        return evalk_2(mval_1, mstate_1, mleftovers_1, lastKState_2, isStopped_1);
                    }
                };
                return (state_3 == null) ? evalmres(Gen_run(m_5)(void 0), void 0, void 0, false) : (isEmpty(state_3.mleftovers) ? (state_3.isStopped ? (new Res$2(1, empty())) : ((kstate_7 = state_3.kstate, (lastMState_2 = state_3.mstate, evalmres(Gen_run(m_5)(lastMState_2), lastMState_2, kstate_7, false))))) : ((isStopped_2 = state_3.isStopped, (lastKState_3 = state_3.kstate, (lastMState_1 = state_3.mstate, (x = (head(state_3.mleftovers) | 0), (xs = tail(state_3.mleftovers), evalk_2(x, lastMState_1, xs, lastKState_3, isStopped_2))))))));
            })))))))(lastKState_4);
            if (matchValue_2.tag === 1) {
                const kvalues_5 = matchValue_2.fields[0];
                return new Res$2(1, kvalues_5);
            }
            else {
                const kvalues_4 = matchValue_2.fields[0];
                const kstate_8 = matchValue_2.fields[1];
                const newState_1 = (kstate_9) => (new BindState$3(mstate_3, kstate_9, mleftovers_3, isStopped_3));
                switch (kstate_8.tag) {
                    case 1: {
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(lastKState_4)));
                    }
                    case 2: {
                        return new Res$2(0, kvalues_4, new LoopState$1(2));
                    }
                    default: {
                        const kstate_10 = kstate_8.fields[0];
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(kstate_10)));
                    }
                }
            }
        };
        const buildSkip_2 = (state_5) => (new LoopState$1(0, state_5));
        const evalk_4 = evalk_3;
        const m_8 = g;
        return new Gen$2(0, (state_6) => {
            let kstate_11, lastMState_5, isStopped_5, lastKState_6, lastMState_4, x_1, xs_1;
            const evalmres_1 = (mres_1, lastMState_3, lastKState_5, isStopped_4) => {
                if (mres_1.tag === 1) {
                    if (isEmpty(mres_1.fields[0])) {
                        return new Res$2(1, empty());
                    }
                    else {
                        const mleftovers_5 = tail(mres_1.fields[0]);
                        const mval_5 = head(mres_1.fields[0]);
                        return evalk_4(mval_5, lastMState_3, mleftovers_5, lastKState_5, isStopped_4);
                    }
                }
                else if (isEmpty(mres_1.fields[0])) {
                    const activePatternResult854_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mstate_5 = activePatternResult854_1;
                    const state_7 = new BindState$3(mstate_5, lastKState_5, empty(), isStopped_4);
                    return new Res$2(0, empty(), buildSkip_2(state_7));
                }
                else {
                    const activePatternResult853_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mleftovers_4 = tail(mres_1.fields[0]);
                    const mstate_4 = activePatternResult853_1;
                    const mval_4 = head(mres_1.fields[0]);
                    return evalk_4(mval_4, mstate_4, mleftovers_4, lastKState_5, isStopped_4);
                }
            };
            return (state_6 == null) ? evalmres_1(Gen_run(m_8)(void 0), void 0, void 0, false) : (isEmpty(state_6.mleftovers) ? (state_6.isStopped ? (new Res$2(1, empty())) : ((kstate_11 = state_6.kstate, (lastMState_5 = state_6.mstate, evalmres_1(Gen_run(m_8)(lastMState_5), lastMState_5, kstate_11, false))))) : ((isStopped_5 = state_6.isStopped, (lastKState_6 = state_6.kstate, (lastMState_4 = state_6.mstate, (x_1 = head(state_6.mleftovers), (xs_1 = tail(state_6.mleftovers), evalk_4(x_1, lastMState_4, xs_1, lastKState_6, isStopped_5))))))));
        });
    })();
}

export function Gen_take(n, g) {
    const builder$0040 = Gen_loop;
    return Gen_BaseBuilder__Delay_1505(builder$0040, () => {
        const evalk_3 = (mval_3, mstate_3, mleftovers_3, lastKState_4, isStopped_3) => {
            let v, evalk_1, buildSkip, evalk_2, m_5, builder$0040_1;
            const matchValue_2 = Gen_run((v = mval_3, (evalk_1 = ((mval, mstate, mleftovers, lastKState_1, isStopped) => {
                let c;
                const matchValue_1 = Gen_run((c = (mval | 0), (c < n) ? Gen_LoopBuilder__Yield_1505(builder$0040, v) : Gen_LoopBuilder__Return_2CC951C7(builder$0040, new Loop_Stop(0))))(lastKState_1);
                if (matchValue_1.tag === 1) {
                    const kvalues_3 = matchValue_1.fields[0];
                    return new Res$2(1, kvalues_3);
                }
                else {
                    const kvalues_2 = matchValue_1.fields[0];
                    const kstate_4 = matchValue_1.fields[1];
                    const newState = (kstate_5) => (new BindState$3(mstate, kstate_5, mleftovers, isStopped));
                    switch (kstate_4.tag) {
                        case 1: {
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(lastKState_1)));
                        }
                        case 2: {
                            return new Res$2(0, kvalues_2, new LoopState$1(2));
                        }
                        default: {
                            const kstate_6 = kstate_4.fields[0];
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(some(kstate_6))));
                        }
                    }
                }
            }), (buildSkip = ((state_2) => (new LoopState$1(0, state_2))), (evalk_2 = evalk_1, (m_5 = ((builder$0040_1 = Gen_feed, Gen_BaseBuilder__Delay_1505(builder$0040_1, () => {
                const m_1 = new Init$1(0, 0);
                return new Gen$2(0, (state) => {
                    let feedback_4, kstate_3, kstate_2;
                    const getInitial = () => {
                        if (m_1.tag === 1) {
                            const f_1 = m_1.fields[0];
                            return f_1() | 0;
                        }
                        else {
                            const m_2 = m_1.fields[0];
                            return m_2 | 0;
                        }
                    };
                    const evalk = (lastFeed, lastKState) => {
                        let curr, tupledArg;
                        const matchValue = Gen_run((curr = (lastFeed | 0), (tupledArg = [curr, curr + 1], Gen_FeedBuilder__Yield_2A0A0(builder$0040_1, tupledArg[0], tupledArg[1]))))(lastKState);
                        if (matchValue.tag === 1) {
                            const kvalues_1 = matchValue.fields[0];
                            return new Res$2(1, kvalues_1);
                        }
                        else {
                            const kvalues = matchValue.fields[0];
                            const kstate = matchValue.fields[1].fields[0];
                            const feedback = matchValue.fields[1].fields[1];
                            let patternInput;
                            switch (feedback.tag) {
                                case 1: {
                                    patternInput = [lastFeed, kstate];
                                    break;
                                }
                                case 2: {
                                    patternInput = [void 0, void 0];
                                    break;
                                }
                                case 3: {
                                    patternInput = [void 0, kstate];
                                    break;
                                }
                                case 4: {
                                    const feedback_2 = feedback.fields[0];
                                    patternInput = [feedback_2, void 0];
                                    break;
                                }
                                default: {
                                    const feedback_1 = feedback.fields[0];
                                    patternInput = [feedback_1, kstate];
                                }
                            }
                            const kstate_1 = patternInput[1];
                            const feedback_3 = patternInput[0];
                            const state_1 = new BindState$3(feedback_3, kstate_1, empty(), false);
                            return new Res$2(0, kvalues, new LoopState$1(0, state_1));
                        }
                    };
                    return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = (state.mstate | 0), (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
                });
            })())), new Gen$2(0, (state_3) => {
                let kstate_7, lastMState_2, isStopped_2, lastKState_3, lastMState_1, x, xs;
                const evalmres = (mres, lastMState, lastKState_2, isStopped_1) => {
                    if (mres.tag === 1) {
                        if (isEmpty(mres.fields[0])) {
                            return new Res$2(1, empty());
                        }
                        else {
                            const mleftovers_2 = tail(mres.fields[0]);
                            const mval_2 = head(mres.fields[0]) | 0;
                            return evalk_2(mval_2, lastMState, mleftovers_2, lastKState_2, isStopped_1);
                        }
                    }
                    else if (isEmpty(mres.fields[0])) {
                        const activePatternResult854 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mstate_2 = activePatternResult854;
                        const state_4 = new BindState$3(mstate_2, lastKState_2, empty(), isStopped_1);
                        return new Res$2(0, empty(), buildSkip(state_4));
                    }
                    else {
                        const activePatternResult853 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mleftovers_1 = tail(mres.fields[0]);
                        const mstate_1 = activePatternResult853;
                        const mval_1 = head(mres.fields[0]) | 0;
                        return evalk_2(mval_1, mstate_1, mleftovers_1, lastKState_2, isStopped_1);
                    }
                };
                return (state_3 == null) ? evalmres(Gen_run(m_5)(void 0), void 0, void 0, false) : (isEmpty(state_3.mleftovers) ? (state_3.isStopped ? (new Res$2(1, empty())) : ((kstate_7 = state_3.kstate, (lastMState_2 = state_3.mstate, evalmres(Gen_run(m_5)(lastMState_2), lastMState_2, kstate_7, false))))) : ((isStopped_2 = state_3.isStopped, (lastKState_3 = state_3.kstate, (lastMState_1 = state_3.mstate, (x = (head(state_3.mleftovers) | 0), (xs = tail(state_3.mleftovers), evalk_2(x, lastMState_1, xs, lastKState_3, isStopped_2))))))));
            })))))))(lastKState_4);
            if (matchValue_2.tag === 1) {
                const kvalues_5 = matchValue_2.fields[0];
                return new Res$2(1, kvalues_5);
            }
            else {
                const kvalues_4 = matchValue_2.fields[0];
                const kstate_8 = matchValue_2.fields[1];
                const newState_1 = (kstate_9) => (new BindState$3(mstate_3, kstate_9, mleftovers_3, isStopped_3));
                switch (kstate_8.tag) {
                    case 1: {
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(lastKState_4)));
                    }
                    case 2: {
                        return new Res$2(0, kvalues_4, new LoopState$1(2));
                    }
                    default: {
                        const kstate_10 = kstate_8.fields[0];
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(kstate_10)));
                    }
                }
            }
        };
        const buildSkip_2 = (state_5) => (new LoopState$1(0, state_5));
        const evalk_4 = evalk_3;
        const m_8 = g;
        return new Gen$2(0, (state_6) => {
            let kstate_11, lastMState_5, isStopped_5, lastKState_6, lastMState_4, x_1, xs_1;
            const evalmres_1 = (mres_1, lastMState_3, lastKState_5, isStopped_4) => {
                if (mres_1.tag === 1) {
                    if (isEmpty(mres_1.fields[0])) {
                        return new Res$2(1, empty());
                    }
                    else {
                        const mleftovers_5 = tail(mres_1.fields[0]);
                        const mval_5 = head(mres_1.fields[0]);
                        return evalk_4(mval_5, lastMState_3, mleftovers_5, lastKState_5, isStopped_4);
                    }
                }
                else if (isEmpty(mres_1.fields[0])) {
                    const activePatternResult854_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mstate_5 = activePatternResult854_1;
                    const state_7 = new BindState$3(mstate_5, lastKState_5, empty(), isStopped_4);
                    return new Res$2(0, empty(), buildSkip_2(state_7));
                }
                else {
                    const activePatternResult853_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mleftovers_4 = tail(mres_1.fields[0]);
                    const mstate_4 = activePatternResult853_1;
                    const mval_4 = head(mres_1.fields[0]);
                    return evalk_4(mval_4, mstate_4, mleftovers_4, lastKState_5, isStopped_4);
                }
            };
            return (state_6 == null) ? evalmres_1(Gen_run(m_8)(void 0), void 0, void 0, false) : (isEmpty(state_6.mleftovers) ? (state_6.isStopped ? (new Res$2(1, empty())) : ((kstate_11 = state_6.kstate, (lastMState_5 = state_6.mstate, evalmres_1(Gen_run(m_8)(lastMState_5), lastMState_5, kstate_11, false))))) : ((isStopped_5 = state_6.isStopped, (lastKState_6 = state_6.kstate, (lastMState_4 = state_6.mstate, (x_1 = head(state_6.mleftovers), (xs_1 = tail(state_6.mleftovers), evalk_4(x_1, lastMState_4, xs_1, lastKState_6, isStopped_5))))))));
        });
    })();
}

export const TopLevelOperators_loop = Gen_loop;

export const TopLevelOperators_feed = Gen_feed;

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
    const builder$0040 = TopLevelOperators_loop;
    return Gen_BaseBuilder__Delay_1505(builder$0040, () => {
        const evalk_3 = (mval_3, mstate_3, mleftovers_3, lastKState_4, isStopped_3) => {
            let i, evalk_1, buildSkip, evalk_2, m_5;
            const matchValue_2 = Gen_run((i = (mval_3 | 0), (evalk_1 = ((mval, mstate, mleftovers, lastKState_1, isStopped) => {
                let value, e, e_1;
                const matchValue_1 = Gen_run((value = mval, (i >= s) ? ((inclEndIdx != null) ? (((e = (inclEndIdx | 0), i > e)) ? ((e_1 = (inclEndIdx | 0), Gen_LoopBuilder__Return_2CC951C7(builder$0040, new Loop_Stop(0)))) : Gen_LoopBuilder__Yield_1505(builder$0040, value)) : Gen_LoopBuilder__Yield_1505(builder$0040, value)) : Gen_LoopBuilder__Zero(builder$0040)))(lastKState_1);
                if (matchValue_1.tag === 1) {
                    const kvalues_3 = matchValue_1.fields[0];
                    return new Res$2(1, kvalues_3);
                }
                else {
                    const kvalues_2 = matchValue_1.fields[0];
                    const kstate_4 = matchValue_1.fields[1];
                    const newState = (kstate_5) => (new BindState$3(mstate, kstate_5, mleftovers, isStopped));
                    switch (kstate_4.tag) {
                        case 1: {
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(lastKState_1)));
                        }
                        case 2: {
                            return new Res$2(0, kvalues_2, new LoopState$1(2));
                        }
                        default: {
                            const kstate_6 = kstate_4.fields[0];
                            return new Res$2(0, kvalues_2, new LoopState$1(0, newState(some(kstate_6))));
                        }
                    }
                }
            }), (buildSkip = ((state_2) => (new LoopState$1(0, state_2))), (evalk_2 = evalk_1, (m_5 = inputGen, new Gen$2(0, (state_3) => {
                let kstate_7, lastMState_2, isStopped_2, lastKState_3, lastMState_1, x_1, xs;
                const evalmres = (mres, lastMState, lastKState_2, isStopped_1) => {
                    if (mres.tag === 1) {
                        if (isEmpty(mres.fields[0])) {
                            return new Res$2(1, empty());
                        }
                        else {
                            const mleftovers_2 = tail(mres.fields[0]);
                            const mval_2 = head(mres.fields[0]);
                            return evalk_2(mval_2, lastMState, mleftovers_2, lastKState_2, isStopped_1);
                        }
                    }
                    else if (isEmpty(mres.fields[0])) {
                        const activePatternResult854 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mstate_2 = activePatternResult854;
                        const state_4 = new BindState$3(mstate_2, lastKState_2, empty(), isStopped_1);
                        return new Res$2(0, empty(), buildSkip(state_4));
                    }
                    else {
                        const activePatternResult853 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mleftovers_1 = tail(mres.fields[0]);
                        const mstate_1 = activePatternResult853;
                        const mval_1 = head(mres.fields[0]);
                        return evalk_2(mval_1, mstate_1, mleftovers_1, lastKState_2, isStopped_1);
                    }
                };
                return (state_3 == null) ? evalmres(Gen_run(m_5)(void 0), void 0, void 0, false) : (isEmpty(state_3.mleftovers) ? (state_3.isStopped ? (new Res$2(1, empty())) : ((kstate_7 = state_3.kstate, (lastMState_2 = state_3.mstate, evalmres(Gen_run(m_5)(lastMState_2), lastMState_2, kstate_7, false))))) : ((isStopped_2 = state_3.isStopped, (lastKState_3 = state_3.kstate, (lastMState_1 = state_3.mstate, (x_1 = head(state_3.mleftovers), (xs = tail(state_3.mleftovers), evalk_2(x_1, lastMState_1, xs, lastKState_3, isStopped_2))))))));
            })))))))(lastKState_4);
            if (matchValue_2.tag === 1) {
                const kvalues_5 = matchValue_2.fields[0];
                return new Res$2(1, kvalues_5);
            }
            else {
                const kvalues_4 = matchValue_2.fields[0];
                const kstate_8 = matchValue_2.fields[1];
                const newState_1 = (kstate_9) => (new BindState$3(mstate_3, kstate_9, mleftovers_3, isStopped_3));
                switch (kstate_8.tag) {
                    case 1: {
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(lastKState_4)));
                    }
                    case 2: {
                        return new Res$2(0, kvalues_4, new LoopState$1(2));
                    }
                    default: {
                        const kstate_10 = kstate_8.fields[0];
                        return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(kstate_10)));
                    }
                }
            }
        };
        const buildSkip_2 = (state_5) => (new LoopState$1(0, state_5));
        const evalk_4 = evalk_3;
        let m_8;
        const builder$0040_1 = Gen_feed;
        m_8 = Gen_BaseBuilder__Delay_1505(builder$0040_1, () => {
            const m_1 = new Init$1(0, 0);
            return new Gen$2(0, (state) => {
                let feedback_4, kstate_3, kstate_2;
                const getInitial = () => {
                    if (m_1.tag === 1) {
                        const f_1 = m_1.fields[0];
                        return f_1() | 0;
                    }
                    else {
                        const m_2 = m_1.fields[0];
                        return m_2 | 0;
                    }
                };
                const evalk = (lastFeed, lastKState) => {
                    let curr, tupledArg;
                    const matchValue = Gen_run((curr = (lastFeed | 0), (tupledArg = [curr, curr + 1], Gen_FeedBuilder__Yield_2A0A0(builder$0040_1, tupledArg[0], tupledArg[1]))))(lastKState);
                    if (matchValue.tag === 1) {
                        const kvalues_1 = matchValue.fields[0];
                        return new Res$2(1, kvalues_1);
                    }
                    else {
                        const kvalues = matchValue.fields[0];
                        const kstate = matchValue.fields[1].fields[0];
                        const feedback = matchValue.fields[1].fields[1];
                        let patternInput;
                        switch (feedback.tag) {
                            case 1: {
                                patternInput = [lastFeed, kstate];
                                break;
                            }
                            case 2: {
                                patternInput = [void 0, void 0];
                                break;
                            }
                            case 3: {
                                patternInput = [void 0, kstate];
                                break;
                            }
                            case 4: {
                                const feedback_2 = feedback.fields[0];
                                patternInput = [feedback_2, void 0];
                                break;
                            }
                            default: {
                                const feedback_1 = feedback.fields[0];
                                patternInput = [feedback_1, kstate];
                            }
                        }
                        const kstate_1 = patternInput[1];
                        const feedback_3 = patternInput[0];
                        const state_1 = new BindState$3(feedback_3, kstate_1, empty(), false);
                        return new Res$2(0, kvalues, new LoopState$1(0, state_1));
                    }
                };
                return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = (state.mstate | 0), (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
            });
        })();
        return new Gen$2(0, (state_6) => {
            let kstate_11, lastMState_5, isStopped_5, lastKState_6, lastMState_4, x_2, xs_1;
            const evalmres_1 = (mres_1, lastMState_3, lastKState_5, isStopped_4) => {
                if (mres_1.tag === 1) {
                    if (isEmpty(mres_1.fields[0])) {
                        return new Res$2(1, empty());
                    }
                    else {
                        const mleftovers_5 = tail(mres_1.fields[0]);
                        const mval_5 = head(mres_1.fields[0]) | 0;
                        return evalk_4(mval_5, lastMState_3, mleftovers_5, lastKState_5, isStopped_4);
                    }
                }
                else if (isEmpty(mres_1.fields[0])) {
                    const activePatternResult854_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mstate_5 = activePatternResult854_1;
                    const state_7 = new BindState$3(mstate_5, lastKState_5, empty(), isStopped_4);
                    return new Res$2(0, empty(), buildSkip_2(state_7));
                }
                else {
                    const activePatternResult853_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mleftovers_4 = tail(mres_1.fields[0]);
                    const mstate_4 = activePatternResult853_1;
                    const mval_4 = head(mres_1.fields[0]) | 0;
                    return evalk_4(mval_4, mstate_4, mleftovers_4, lastKState_5, isStopped_4);
                }
            };
            return (state_6 == null) ? evalmres_1(Gen_run(m_8)(void 0), void 0, void 0, false) : (isEmpty(state_6.mleftovers) ? (state_6.isStopped ? (new Res$2(1, empty())) : ((kstate_11 = state_6.kstate, (lastMState_5 = state_6.mstate, evalmres_1(Gen_run(m_8)(lastMState_5), lastMState_5, kstate_11, false))))) : ((isStopped_5 = state_6.isStopped, (lastKState_6 = state_6.kstate, (lastMState_4 = state_6.mstate, (x_2 = (head(state_6.mleftovers) | 0), (xs_1 = tail(state_6.mleftovers), evalk_4(x_2, lastMState_4, xs_1, lastKState_6, isStopped_5))))))));
        });
    })();
}

