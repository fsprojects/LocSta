import { Record, Union } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Types.js";
import { class_type, record_type, bool_type, unit_type, list_type, union_type, lambda_type, option_type } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Reflection.js";
import { append, head, tail, isEmpty, empty, singleton } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/List.js";
import { defaultArg, defaultArgWith, value as value_1, some } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Option.js";
import { equals, getEnumerator } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Util.js";
import { map, truncate, toList, enumerateWhile, delay } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Seq.js";

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

export function Gen_createGen(f) {
    return new Gen$2(0, f);
}

export function Gen_createLoop(f) {
    return new Gen$2(0, f);
}

export function Gen_createFeed(f) {
    return new Gen$2(0, f);
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

export function Gen_bindLoopWhateverGen(evalk, buildSkip, createWhatever, m) {
    return createWhatever((state) => {
        let kstate, lastMState_2, isStopped_1, lastKState_1, lastMState_1, x, xs;
        const evalmres = (mres, lastMState, lastKState, isStopped) => {
            if (mres.tag === 1) {
                if (isEmpty(mres.fields[0])) {
                    return new Res$2(1, empty());
                }
                else {
                    const mleftovers_1 = tail(mres.fields[0]);
                    const mval_1 = head(mres.fields[0]);
                    return evalk(mval_1, lastMState, mleftovers_1, lastKState, isStopped);
                }
            }
            else if (isEmpty(mres.fields[0])) {
                const activePatternResult28900 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                const mstate_1 = activePatternResult28900;
                const state_1 = new BindState$3(mstate_1, lastKState, empty(), isStopped);
                return new Res$2(0, empty(), buildSkip(state_1));
            }
            else {
                const activePatternResult28899 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                const mleftovers = tail(mres.fields[0]);
                const mstate = activePatternResult28899;
                const mval = head(mres.fields[0]);
                return evalk(mval, mstate, mleftovers, lastKState, isStopped);
            }
        };
        return (state == null) ? evalmres(Gen_run(m)(void 0), void 0, void 0, false) : (isEmpty(state.mleftovers) ? (state.isStopped ? (new Res$2(1, empty())) : ((kstate = state.kstate, (lastMState_2 = state.mstate, evalmres(Gen_run(m)(lastMState_2), lastMState_2, kstate, false))))) : ((isStopped_1 = state.isStopped, (lastKState_1 = state.kstate, (lastMState_1 = state.mstate, (x = head(state.mleftovers), (xs = tail(state.mleftovers), evalk(x, lastMState_1, xs, lastKState_1, isStopped_1))))))));
    });
}

export function Gen_bind(k, m) {
    const evalk = (mval, mstate, mleftovers, lastKState, isStopped) => {
        const matchValue = Gen_run(k(mval))(lastKState);
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
    };
    const buildSkip = (state) => (new LoopState$1(0, state));
    return Gen_bindLoopWhateverGen(evalk, buildSkip, (f) => Gen_createLoop(f), m);
}

export function Gen_bindLoopFeedFeed(k, m) {
    const evalk = (mval, mstate, mleftovers, lastKState, isStopped) => {
        const matchValue = Gen_run(k(mval))(lastKState);
        if (matchValue.tag === 1) {
            const kvalues_1 = matchValue.fields[0];
            return new Res$2(1, kvalues_1);
        }
        else {
            const kvalues = matchValue.fields[0];
            const kstate = matchValue.fields[1].fields[0];
            const feedState = matchValue.fields[1].fields[1];
            const state = new BindState$3(mstate, kstate, mleftovers, isStopped);
            return new Res$2(0, kvalues, new FeedState$2(0, state, feedState));
        }
    };
    const buildSkip = (state_1) => (new FeedState$2(0, some(state_1), new FeedType$1(1)));
    return Gen_bindLoopWhateverGen(evalk, buildSkip, (f) => Gen_createFeed(f), m);
}

export function Gen_bindInitFeedLoop(k, m) {
    return Gen_createLoop((state) => {
        let feedback_4, kstate_3, kstate_2;
        const getInitial = () => {
            if (m.tag === 1) {
                const f = m.fields[0];
                return f();
            }
            else {
                const m_1 = m.fields[0];
                return m_1;
            }
        };
        const evalk = (lastFeed, lastKState) => {
            const matchValue = Gen_run(k(lastFeed))(lastKState);
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
                        patternInput = [some(lastFeed), kstate];
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
                        patternInput = [some(feedback_2), void 0];
                        break;
                    }
                    default: {
                        const feedback_1 = feedback.fields[0];
                        patternInput = [some(feedback_1), kstate];
                    }
                }
                const kstate_1 = patternInput[1];
                const feedback_3 = patternInput[0];
                const state_1 = new BindState$3(feedback_3, kstate_1, empty(), false);
                return new Res$2(0, kvalues, new LoopState$1(0, state_1));
            }
        };
        return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = value_1(state.mstate), (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
    });
}

export function Gen_ofRepeatingValues(values) {
    return Gen_createLoop((_arg1) => (new Res$2(0, values, new LoopState$1(1))));
}

export function Gen_ofRepeatingValue(value) {
    return Gen_ofRepeatingValues(singleton(value));
}

export function Gen_ofOneTimeValues(values) {
    return Gen_createLoop((_arg1) => (new Res$2(1, values)));
}

export function Gen_ofOneTimeValue(value) {
    return Gen_ofOneTimeValues(singleton(value));
}

export function Gen_ofSeqOneByOne(s) {
    return Gen_createLoop((enumerator) => {
        const enumerator_1 = defaultArgWith(enumerator, () => getEnumerator(s));
        return enumerator_1["System.Collections.IEnumerator.MoveNext"]() ? Res_Loop_emit(enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"](), enumerator_1) : Res_Loop_stop();
    });
}

export function Gen_ofListOneByOne(list) {
    return Gen_createLoop((l) => {
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
    return Gen_createLoop((_arg1) => {
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

export function Gen_combineLoop(a, b) {
    return Gen_createLoop((state) => {
        const state_1 = defaultArg(state, new Gen_CombineInfo$2(void 0, void 0));
        const ares = Gen_run(a)(state_1.astate);
        if (ares.tag === 1) {
            const avalues_1 = ares.fields[0];
            return new Res$2(1, avalues_1);
        }
        else {
            const avalues = ares.fields[0];
            const astate = Gen_$007CLoopStateToOption$007C(state_1.astate, ares.fields[1]);
            const bres = Gen_run(b())(state_1.bstate);
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

export function Gen_combineFeed(a, b) {
    return Gen_createFeed((state) => {
        const state_1 = defaultArg(state, new Gen_CombineInfo$2(void 0, void 0));
        const matchValue = Gen_run(a)(state_1.astate);
        if (matchValue.tag === 1) {
            const avalues_1 = matchValue.fields[0];
            return new Res$2(1, avalues_1);
        }
        else {
            const avalues = matchValue.fields[0];
            const astate = matchValue.fields[1].fields[0];
            const afeedback = matchValue.fields[1].fields[1];
            const matchValue_1 = Gen_run(b())(state_1.bstate);
            if (matchValue_1.tag === 1) {
                const bvalues_1 = matchValue_1.fields[0];
                return new Res$2(1, append(avalues, bvalues_1));
            }
            else {
                const bvalues = matchValue_1.fields[0];
                const bstate = matchValue_1.fields[1].fields[0];
                const bfeedback = matchValue_1.fields[1].fields[1];
                const b_1 = matchValue_1;
                const finalFeedback = equals(b_1, Res_Feed_zero()) ? afeedback : bfeedback;
                const state_2 = new Gen_CombineInfo$2(astate, bstate);
                return new Res$2(0, append(avalues, bvalues), new FeedState$2(0, state_2, finalFeedback));
            }
        }
    });
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

export function Gen_Evaluable$1__GetNext(_) {
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

export function Gen_toSeqFx(fx) {
    let state = void 0;
    let resume = true;
    return (inputValues) => {
        const enumerator = getEnumerator(inputValues);
        return delay(() => enumerateWhile(() => (resume ? enumerator["System.Collections.IEnumerator.MoveNext"]() : false), delay(() => {
            const matchValue = Gen_run(fx(enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]()))(state);
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
    };
}

export function Gen_toList(gen) {
    return toList(Gen_toSeq(gen));
}

export function Gen_toListn(count, gen) {
    return toList(truncate(count, Gen_toSeq(gen)));
}

export function Gen_toListFx(fx, input) {
    return toList(Gen_toSeqFx(fx)(input));
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

export function Gen_BaseBuilder__Run_FCFD9EF(_, delayed) {
    return delayed();
}

export function Gen_BaseBuilder__For_Z48B64FBB(_, list, body) {
    return Gen_ofListAllAtOnce(Gen_toListFx(body, list));
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

export function Gen_LoopBuilder__Bind_Z7EFF1A0D(_, m, f) {
    return Gen_bind(f, m);
}

export function Gen_LoopBuilder__Combine_463FDD0A(_, x, delayed) {
    return Gen_combineLoop(x, delayed);
}

export function Gen_LoopBuilder__Zero(_) {
    const res = Res_Loop_zero();
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Yield_1505(_, value) {
    const res = Res_Loop_emitAndKeepLast(value);
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_Z531701A5(_, _arg1) {
    const value = _arg1.fields[0];
    const res = Res_Loop_emitAndKeepLast(value);
    return Gen_createLoop((_arg1_1) => res);
}

export function Gen_LoopBuilder__Return_Z3449CE5B(_, _arg2) {
    const value = _arg2.fields[0];
    const res = Res_Loop_emitAndReset(value);
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_ZAE17398(_, _arg3) {
    const value = _arg3.fields[0];
    const res = Res_Loop_emitAndStop(value);
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_Z7BD98500(_, _arg4) {
    const values = _arg4.fields[0];
    const res = Res_Loop_emitManyAndKeepLast(values);
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_Z48160702(_, _arg5) {
    const values = _arg5.fields[0];
    const res = Res_Loop_emitManyAndReset(values);
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_26201B93(_, _arg6) {
    const values = _arg6.fields[0];
    const res = Res_Loop_emitManyAndStop(values);
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_2CC912DE(_, _arg7) {
    const res = Res_Loop_skipAndKeepLast();
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_3CCCF4A0(_, _arg8) {
    const res = Res_Loop_skipAndReset();
    return Gen_createLoop((_arg1) => res);
}

export function Gen_LoopBuilder__Return_2CC951C7(_, _arg9) {
    const res = Res_Loop_stop();
    return Gen_createLoop((_arg1) => res);
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

export function Gen_FeedBuilder__Bind_Z7B4A5563(_, m, f) {
    return Gen_bindInitFeedLoop(f, m);
}

export function Gen_FeedBuilder__Bind_Z7EFF1A0D(_, m, f) {
    return Gen_bind(f, m);
}

export function Gen_FeedBuilder__Bind_3CE93C44(_, m, f) {
    return Gen_bindLoopFeedFeed(f, m);
}

export function Gen_FeedBuilder__Combine_463FDD0A(_, x, delayed) {
    return Gen_combineLoop(x, delayed);
}

export function Gen_FeedBuilder__Combine_2B1CB22A(_, x, delayed) {
    return Gen_combineFeed(x, delayed);
}

export function Gen_FeedBuilder__Zero(_) {
    const res = Res_Feed_zero();
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Yield_2A0A0(_, value, feedback) {
    const res = Res_Feed_emit(value, feedback);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z2DCF3B94(_, _arg1) {
    const value = _arg1.fields[0];
    const feedback = _arg1.fields[1];
    const res = Res_Feed_emit(value, feedback);
    return Gen_createFeed((_arg1_1) => res);
}

export function Gen_FeedBuilder__Return_16BB62BF(_, _arg2) {
    const value = _arg2.fields[0];
    const res = Res_Feed_emitAndKeepLast(value);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z73E685(_, _arg3) {
    const value = _arg3.fields[0];
    const res = Res_Feed_emitAndReset(value);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_52CCB8D2(_, _arg4) {
    const value = _arg4.fields[0];
    const res = Res_Feed_emitAndResetFeedback(value);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_7C29FB64(_, _arg5) {
    const value = _arg5.fields[0];
    const feedback = _arg5.fields[1];
    const res = Res_Feed_emitAndResetDescendants(value, feedback);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z5BEBAC8A(_, _arg6) {
    const value = _arg6.fields[0];
    const res = Res_Feed_emitAndStop(value);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z6686A729(_, _arg7) {
    const values = _arg7.fields[0];
    const feedback = _arg7.fields[1];
    const res = Res_Feed_emitMany(values, feedback);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_12885A84(_, _arg8) {
    const values = _arg8.fields[0];
    const res = Res_Feed_emitManyAndKeepLast(values);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z2ED10DE0(_, _arg9) {
    const values = _arg9.fields[0];
    const res = Res_Feed_emitManyAndReset(values);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z5D1E60B7(_, _arg10) {
    const values = _arg10.fields[0];
    const res = Res_Feed_emitManyAndResetFeedback(values);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z57703541(_, _arg11) {
    const values = _arg11.fields[0];
    const feedback = _arg11.fields[1];
    const res = Res_Feed_emitManyAndResetDescendants(values, feedback);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_3C05DB8D(_, _arg12) {
    const values = _arg12.fields[0];
    const res = Res_Feed_emitManyAndStop(values);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z46B6E02F(_, _arg13) {
    const feedback = _arg13.fields[0];
    const res = Res_Feed_skip(feedback);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_1F197D3A(_, _arg14) {
    const res = Res_Feed_skipAndKeepLast();
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_435D107E(_, _arg15) {
    const res = Res_Feed_skipAndReset();
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z2C47D729(_, _arg16) {
    const res = Res_Feed_skipAndResetFeedback();
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_18629FD9(_, _arg17) {
    const feedback = _arg17.fields[0];
    const res = Res_Feed_skipAndResetDescendants(feedback);
    return Gen_createFeed((_arg1) => res);
}

export function Gen_FeedBuilder__Return_Z3B1223E7(_, _arg18) {
    const res = Res_Feed_stop();
    return Gen_createFeed((_arg1) => res);
}

export const Gen_loop = Gen_LoopBuilder_$ctor();

export const Gen_feed = Gen_FeedBuilder_$ctor();

export function Gen_pipe(g, f) {
    const builder$0040 = Gen_loop;
    return Gen_BaseBuilder__Run_FCFD9EF(builder$0040, Gen_BaseBuilder__Delay_1505(builder$0040, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(builder$0040, f, (_arg1) => {
        const f$0027 = _arg1;
        return Gen_BaseBuilder__ReturnFrom_1505(builder$0040, g(f$0027));
    })));
}

export function Gen_pipeFx(g, f, x) {
    const builder$0040 = Gen_loop;
    return Gen_BaseBuilder__Run_FCFD9EF(builder$0040, Gen_BaseBuilder__Delay_1505(builder$0040, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(builder$0040, f(x), (_arg1) => {
        const f$0027 = _arg1;
        return Gen_BaseBuilder__ReturnFrom_1505(builder$0040, g(f$0027));
    })));
}

export function Gen_map2(proj, inputGen) {
    return Gen_createLoop((state) => {
        const mapValues = (values, state_1) => toList(delay(() => map((v) => proj(v, state_1), values)));
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

export function Gen_map(proj, inputGen) {
    return Gen_map2((v, _arg1) => proj(v), inputGen);
}

export function Gen_apply(xGen, fGen) {
    const builder$0040 = Gen_loop;
    return Gen_BaseBuilder__Run_FCFD9EF(builder$0040, Gen_BaseBuilder__Delay_1505(builder$0040, () => Gen_LoopBuilder__Bind_Z7EFF1A0D(builder$0040, xGen, (_arg1) => {
        const l$0027 = _arg1;
        return Gen_LoopBuilder__Bind_Z7EFF1A0D(builder$0040, fGen, (_arg2) => {
            const f$0027 = _arg2;
            const result = f$0027(l$0027);
            return Gen_LoopBuilder__Yield_1505(builder$0040, result);
        });
    })));
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

export function TopLevelOperators_op_GreaterEqualsGreater(f, g) {
    return (x) => Gen_pipeFx(g, f, x);
}

export function TopLevelOperators_op_BarEqualsGreater(f, g) {
    return Gen_pipe(g, f);
}

export function TopLevelOperators_op_GreaterGreaterEquals(m, f) {
    return Gen_bind(f, m);
}

export const TopLevelOperators_loop = Gen_loop;

export const TopLevelOperators_feed = Gen_feed;

