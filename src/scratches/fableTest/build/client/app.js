import { Union } from "./.fable/fable-library.3.2.9/Types.js";
import { class_type, union_type, int32_type } from "./.fable/fable-library.3.2.9/Reflection.js";
import { structuralHash, equals, getEnumerator, createAtom } from "./.fable/fable-library.3.2.9/Util.js";
import { contains, map, delay } from "./.fable/fable-library.3.2.9/Seq.js";
import { rangeDouble } from "./.fable/fable-library.3.2.9/Range.js";
import { value as value_2, some, defaultArgWith } from "./.fable/fable-library.3.2.9/Option.js";
import { Gen_Evaluable$1__Evaluate, Gen_feed, Gen_FeedBuilder__Yield_2A0A0, Init$1, Gen_BaseBuilder__ReturnFrom_1505, Gen_toEvaluable, Gen_$007CLoopStateToOption$007C, LoopState$1, BindState$3, Res$2, Gen_LoopBuilder__Yield_1505, TopLevelOperators_loop, Gen_LoopBuilder__Zero, Gen_LoopBuilder__Combine_463FDD0A, Gen_run, Gen_BaseBuilder__Delay_1505, Gen$2, Res_Loop_emit } from "./core.js";
import { printf, toConsole } from "./.fable/fable-library.3.2.9/String.js";
import { head, tail, empty, isEmpty } from "./.fable/fable-library.3.2.9/List.js";

export class ElementId extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Id"];
    }
}

export function ElementId$reflection() {
    return union_type("App.ElementId", [], ElementId, () => [[["Item", int32_type]]]);
}

export class App {
    constructor(appElement, triggerUpdate) {
        this.appElement = appElement;
        this["triggerUpdate@11"] = triggerUpdate;
        this.id = -1;
        this.currentTrigger = (void 0);
    }
}

export function App$reflection() {
    return class_type("App.App", void 0, App);
}

export function App_$ctor_Z7E266366(appElement, triggerUpdate) {
    return new App(appElement, triggerUpdate);
}

export function App__createElement_Z721C83C5(_, name) {
    _.id = ((_.id + 1) | 0);
    return [document.createElement(name), new ElementId(0, _.id)];
}

export function App__run(_) {
    const element = _["triggerUpdate@11"](void 0);
    void _.appElement.appendChild(element);
}

export function App__triggerUpdate_376E9F39(_, id) {
    const element = _["triggerUpdate@11"](id);
}

export let app = createAtom(null);

export function toSeq(coll) {
    return delay(() => map((i) => (coll[i]), rangeDouble(0, 1, coll.length - 1)));
}

export function elem(name, attributes, child) {
    return new Gen$2(0, (state) => {
        const patternInput = defaultArgWith(state, () => App__createElement_Z721C83C5(app(), name));
        const id = patternInput[1];
        const elem_1 = patternInput[0];
        const enumerator = getEnumerator(attributes);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const forLoopVar = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                const avalue = forLoopVar[1];
                const aname = forLoopVar[0];
                const elemAttr = elem_1.attributes.getNamedItem(aname);
                if (elemAttr.value !== avalue) {
                    elemAttr.value = avalue;
                }
            }
        }
        finally {
            enumerator.Dispose();
        }
        if (!contains(child, toSeq(elem_1.childNodes), {
            Equals: (x, y) => equals(x, y),
            GetHashCode: (x) => structuralHash(x),
        })) {
            void elem_1.appendChild(child);
        }
        return Res_Loop_emit(elem_1, [elem_1, id]);
    });
}

export function text(content) {
    return document.createTextNode(content);
}

export function div(attributes, content) {
    return elem("div", attributes, content);
}

export function p(attributes, content) {
    return elem("p", attributes, content);
}

export function button(content, click) {
    return Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => {
        const evalk = (mval, mstate, mleftovers, lastKState, isStopped) => {
            let button_1, button_2, click_1;
            const matchValue = Gen_run((button_1 = mval, (button_2 = button_1, Gen_LoopBuilder__Combine_463FDD0A(TopLevelOperators_loop, (click == null) ? ((toConsole(printf("no click")), (button_2.onclick = ((_arg1_1) => {
            }), Gen_LoopBuilder__Zero(TopLevelOperators_loop)))) : ((click_1 = click, (toConsole(printf("register click")), (button_2.onclick = click_1, Gen_LoopBuilder__Zero(TopLevelOperators_loop))))), Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, button_2))))))(lastKState);
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
        const evalk_1 = evalk;
        const m_2 = elem("button", [], text(content));
        return new Gen$2(0, (state_1) => {
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
        });
    })();
}

export const view = Gen_toEvaluable(Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => {
    const evalk_3 = (mval_3, mstate_3, mleftovers_3, lastKState_4, isStopped_3) => {
        let c, evalk_1, buildSkip, evalk_2, m_5;
        const matchValue_2 = Gen_run((c = mval_3, (evalk_1 = ((mval, mstate, mleftovers, lastKState_1, isStopped) => {
            let button_1;
            const matchValue_1 = Gen_run((button_1 = mval, Gen_BaseBuilder__ReturnFrom_1505(TopLevelOperators_loop, div([], button_1))))(lastKState_1);
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
        }), (buildSkip = ((state_2) => (new LoopState$1(0, state_2))), (evalk_2 = evalk_1, (m_5 = button("Increment", (args) => {
            toConsole(printf("Clicked"));
        }), new Gen$2(0, (state_3) => {
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
                    return new Res$2(0, kvalues_4, new LoopState$1(0, newState_1(some(kstate_10))));
                }
            }
        }
    };
    const buildSkip_2 = (state_5) => (new LoopState$1(0, state_5));
    const evalk_4 = evalk_3;
    const m_8 = Gen_BaseBuilder__Delay_1505(Gen_feed, () => {
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
                const matchValue = Gen_run((curr = (lastFeed | 0), (tupledArg = [curr, curr + 1], Gen_FeedBuilder__Yield_2A0A0(Gen_feed, tupledArg[0], tupledArg[1]))))(lastKState);
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
})());

app(App_$ctor_Z7E266366(document.querySelector("#app"), (_arg1) => value_2(Gen_Evaluable$1__Evaluate(view))), true);

App__run(app());

//# sourceMappingURL=app.js.map
