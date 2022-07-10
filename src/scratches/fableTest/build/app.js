import { Union } from "./.fable/fable-library.3.2.9/Types.js";
import { class_type, union_type, int32_type } from "./.fable/fable-library.3.2.9/Reflection.js";
import { toText, printf, interpolate, toConsole } from "./.fable/fable-library.3.2.9/String.js";
import { structuralHash, equals, getEnumerator, safeHash, createAtom } from "./.fable/fable-library.3.2.9/Util.js";
import { toList, contains, map, delay } from "./.fable/fable-library.3.2.9/Seq.js";
import { rangeDouble } from "./.fable/fable-library.3.2.9/Range.js";
import { Gen_Evaluable$1__Evaluate, Gen_toEvaluable, Gen_ofMutable, Gen_BaseBuilder__ReturnFrom_1505, Gen_initWith, Gen_LoopBuilder__Yield_1505, TopLevelOperators_loop, Loop_Skip, Gen_LoopBuilder__Return_2CC912DE, LoopState$1, Gen_$007CLoopStateToOption$007C, Res_Loop_emitManyAndStop, FeedType$1, BindState$3, Gen$2, FeedState$2, Res_Feed_zero, Res$2, Gen_CombineInfo$2, Feed_Emit$2, Gen_FeedBuilder__Return_Z2DCF3B94, TopLevelOperators_feed, Gen_FeedBuilder__Zero, Gen_run, Init$1, Gen_BaseBuilder__Delay_1505 } from "./core.js";
import { value as value_3, some, defaultArg } from "./.fable/fable-library.3.2.9/Option.js";
import { head, tail, empty, isEmpty, append } from "./.fable/fable-library.3.2.9/List.js";

export class Sender extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Id"];
    }
}

export function Sender$reflection() {
    return union_type("App.Sender", [], Sender, () => [[["Item", int32_type]]]);
}

export class App {
    constructor(appElement, triggerUpdate) {
        this.appElement = appElement;
        this.triggerUpdate = triggerUpdate;
        this.currId = -1;
        this["CurrentSender@"] = (void 0);
    }
}

export function App$reflection() {
    return class_type("App.App", void 0, App);
}

export function App_$ctor_3C4DE8D7(appElement, triggerUpdate) {
    return new App(appElement, triggerUpdate);
}

export function App__get_CurrentSender(__) {
    return __["CurrentSender@"];
}

export function App__set_CurrentSender_B088534(__, v) {
    __["CurrentSender@"] = v;
}

export function App__NewSender(_) {
    _.currId = ((_.currId + 1) | 0);
    toConsole(interpolate("New sender: %P()", [_.currId]));
    return new Sender(0, _.currId);
}

export function App__CreateElement_Z721C83C5(_, name) {
    toConsole(interpolate("Create: %P()", [name]));
    return document.createElement(name);
}

export function App__Run(_) {
    const initialElement = _.triggerUpdate(void 0);
    if (initialElement == null) {
        toConsole(printf("NO INITIAL ELEMENT GIVEN"));
    }
    else {
        const element = initialElement;
        void _.appElement.appendChild(element);
    }
}

export function App__TriggerUpdate_B088534(this$, sender) {
    App__set_CurrentSender_B088534(this$, sender);
    toConsole(interpolate("Trigger update with sender: %P()", [sender]));
    const element = this$.triggerUpdate(sender);
    if (element != null) {
        const element_1 = element;
    }
    else {
        toConsole(printf("NONE"));
    }
}

export let app = createAtom(null);

export function toSeq(coll) {
    return delay(() => map((i) => (coll[i]), rangeDouble(0, 1, coll.length - 1)));
}

export function elem(name, attributes, child) {
    return Gen_BaseBuilder__Delay_1505(TopLevelOperators_feed, () => {
        const m_4 = new Init$1(1, () => App__CreateElement_Z721C83C5(app(), name));
        return new Gen$2(0, (state_10) => {
            let feedback_4, kstate_5, kstate_4;
            const getInitial = () => {
                if (m_4.tag === 1) {
                    const f_8 = m_4.fields[0];
                    return f_8();
                }
                else {
                    const m_5 = m_4.fields[0];
                    return m_5;
                }
            };
            const evalk_2 = (lastFeed, lastKState_3) => {
                let elem_1, evalk, buildSkip, evalk_1, m_2;
                const matchValue_4 = Gen_run((elem_1 = lastFeed, (toConsole(interpolate("Eval: %P() (%P())", [name, safeHash(elem_1)])), (evalk = ((mval, mstate, mleftovers, lastKState, isStopped) => {
                    let child_1, enumerator, x_2, delayed;
                    const matchValue_3 = Gen_run((child_1 = mval, ((enumerator = getEnumerator(attributes), (() => {
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
                    })()), (x_2 = ((!contains(child_1, toSeq(elem_1.childNodes), {
                        Equals: (x_1, y) => equals(x_1, y),
                        GetHashCode: (x_1) => structuralHash(x_1),
                    })) ? ((toConsole(interpolate("add child (node count = %P())", [elem_1.childNodes.length])), (void elem_1.appendChild(child_1), Gen_FeedBuilder__Zero(TopLevelOperators_feed)))) : Gen_FeedBuilder__Zero(TopLevelOperators_feed)), (delayed = Gen_BaseBuilder__Delay_1505(TopLevelOperators_feed, () => Gen_FeedBuilder__Return_Z2DCF3B94(TopLevelOperators_feed, new Feed_Emit$2(0, elem_1, elem_1))), new Gen$2(0, (state_3) => {
                        const state_4 = defaultArg(state_3, new Gen_CombineInfo$2(void 0, void 0));
                        const matchValue_1 = Gen_run(x_2)(state_4.astate);
                        if (matchValue_1.tag === 1) {
                            const avalues_1 = matchValue_1.fields[0];
                            return new Res$2(1, avalues_1);
                        }
                        else {
                            const avalues = matchValue_1.fields[0];
                            const astate = matchValue_1.fields[1].fields[0];
                            const afeedback = matchValue_1.fields[1].fields[1];
                            const matchValue_2 = Gen_run(delayed())(state_4.bstate);
                            if (matchValue_2.tag === 1) {
                                const bvalues_1 = matchValue_2.fields[0];
                                return new Res$2(1, append(avalues, bvalues_1));
                            }
                            else {
                                const bvalues = matchValue_2.fields[0];
                                const bstate = matchValue_2.fields[1].fields[0];
                                const bfeedback = matchValue_2.fields[1].fields[1];
                                const b_1 = matchValue_2;
                                const finalFeedback = equals(b_1, Res_Feed_zero()) ? afeedback : bfeedback;
                                const state_5 = new Gen_CombineInfo$2(astate, bstate);
                                return new Res$2(0, append(avalues, bvalues), new FeedState$2(0, state_5, finalFeedback));
                            }
                        }
                    }))))))(lastKState);
                    if (matchValue_3.tag === 1) {
                        const kvalues_1 = matchValue_3.fields[0];
                        return new Res$2(1, kvalues_1);
                    }
                    else {
                        const kvalues = matchValue_3.fields[0];
                        const kstate = matchValue_3.fields[1].fields[0];
                        const feedState = matchValue_3.fields[1].fields[1];
                        const state_6 = new BindState$3(mstate, kstate, mleftovers, isStopped);
                        return new Res$2(0, kvalues, new FeedState$2(0, state_6, feedState));
                    }
                }), (buildSkip = ((state_7) => (new FeedState$2(0, some(state_7), new FeedType$1(1)))), (evalk_1 = evalk, (m_2 = (new Gen$2(0, (state) => {
                    const mapValues = (values, state_1) => toList(delay(() => map((v_1) => v_1, values)));
                    const matchValue = Gen_run(child)(state);
                    if (matchValue.tag === 1) {
                        const values_2 = matchValue.fields[0];
                        return Res_Loop_emitManyAndStop(mapValues(values_2, void 0));
                    }
                    else {
                        const values_1 = matchValue.fields[0];
                        const state_2 = matchValue.fields[1];
                        return new Res$2(0, mapValues(values_1, state_2), state_2);
                    }
                })), new Gen$2(0, (state_8) => {
                    let kstate_1, lastMState_2, isStopped_2, lastKState_2, lastMState_1, x_3, xs;
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
                            const state_9 = new BindState$3(mstate_2, lastKState_1, empty(), isStopped_1);
                            return new Res$2(0, empty(), buildSkip(state_9));
                        }
                        else {
                            const activePatternResult853 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                            const mleftovers_1 = tail(mres.fields[0]);
                            const mstate_1 = activePatternResult853;
                            const mval_1 = head(mres.fields[0]);
                            return evalk_1(mval_1, mstate_1, mleftovers_1, lastKState_1, isStopped_1);
                        }
                    };
                    return (state_8 == null) ? evalmres(Gen_run(m_2)(void 0), void 0, void 0, false) : (isEmpty(state_8.mleftovers) ? (state_8.isStopped ? (new Res$2(1, empty())) : ((kstate_1 = state_8.kstate, (lastMState_2 = state_8.mstate, evalmres(Gen_run(m_2)(lastMState_2), lastMState_2, kstate_1, false))))) : ((isStopped_2 = state_8.isStopped, (lastKState_2 = state_8.kstate, (lastMState_1 = state_8.mstate, (x_3 = head(state_8.mleftovers), (xs = tail(state_8.mleftovers), evalk_1(x_3, lastMState_1, xs, lastKState_2, isStopped_2))))))));
                }))))))))(lastKState_3);
                if (matchValue_4.tag === 1) {
                    const kvalues_3 = matchValue_4.fields[0];
                    return new Res$2(1, kvalues_3);
                }
                else {
                    const kvalues_2 = matchValue_4.fields[0];
                    const kstate_2 = matchValue_4.fields[1].fields[0];
                    const feedback = matchValue_4.fields[1].fields[1];
                    let patternInput;
                    switch (feedback.tag) {
                        case 1: {
                            patternInput = [some(lastFeed), kstate_2];
                            break;
                        }
                        case 2: {
                            patternInput = [void 0, void 0];
                            break;
                        }
                        case 3: {
                            patternInput = [void 0, kstate_2];
                            break;
                        }
                        case 4: {
                            const feedback_2 = feedback.fields[0];
                            patternInput = [some(feedback_2), void 0];
                            break;
                        }
                        default: {
                            const feedback_1 = feedback.fields[0];
                            patternInput = [some(feedback_1), kstate_2];
                        }
                    }
                    const kstate_3 = patternInput[1];
                    const feedback_3 = patternInput[0];
                    const state_11 = new BindState$3(feedback_3, kstate_3, empty(), false);
                    return new Res$2(0, kvalues_2, new LoopState$1(0, state_11));
                }
            };
            return (state_10 == null) ? evalk_2(getInitial(), void 0) : (state_10.isStopped ? (new Res$2(1, empty())) : ((state_10.mstate != null) ? ((feedback_4 = value_3(state_10.mstate), (kstate_5 = state_10.kstate, evalk_2(feedback_4, kstate_5)))) : ((kstate_4 = state_10.kstate, evalk_2(getInitial(), kstate_4)))));
        });
    })();
}

export function text(content) {
    return Gen_BaseBuilder__Delay_1505(TopLevelOperators_feed, () => {
        const m_1 = new Init$1(1, () => document.createTextNode(content));
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
                let elem_1;
                const matchValue = Gen_run((elem_1 = lastFeed, ((elem_1.textContent !== content) ? (elem_1.textContent = content) : (void 0), Gen_FeedBuilder__Return_Z2DCF3B94(TopLevelOperators_feed, new Feed_Emit$2(0, elem_1, elem_1)))))(lastKState);
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
            return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = value_3(state.mstate), (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
        });
    })();
}

export function div(attributes, content) {
    return elem("div", attributes, content);
}

export function p(attributes, content) {
    return elem("p", attributes, content);
}

export function button(content, click) {
    return Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => {
        const evalk_2 = (mval_3, mstate_3, mleftovers_3, lastKState_3, isStopped_3) => {
            let clickId, evalk, buildSkip, evalk_1, m_2, inputGen;
            const matchValue_2 = Gen_run((clickId = mval_3, equals(App__get_CurrentSender(app()), clickId) ? ((App__set_CurrentSender_B088534(app(), void 0), (click(), (toConsole(printf("SKIP")), Gen_LoopBuilder__Return_2CC912DE(TopLevelOperators_loop, new Loop_Skip(0)))))) : ((evalk = ((mval, mstate, mleftovers, lastKState, isStopped) => {
                let button_1;
                const matchValue_1 = Gen_run((button_1 = mval, (button_1.onclick = ((_arg1_2) => {
                    toConsole(printf("-----CLICK"));
                    App__TriggerUpdate_B088534(app(), clickId);
                }), Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, button_1))))(lastKState);
                if (matchValue_1.tag === 1) {
                    const kvalues_1 = matchValue_1.fields[0];
                    return new Res$2(1, kvalues_1);
                }
                else {
                    const kvalues = matchValue_1.fields[0];
                    const kstate = matchValue_1.fields[1];
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
            }), (buildSkip = ((state_3) => (new LoopState$1(0, state_3))), (evalk_1 = evalk, (m_2 = ((inputGen = elem("button", [], content), new Gen$2(0, (state) => {
                const mapValues = (values, state_1) => toList(delay(() => map((v_1) => v_1, values)));
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
            }))), new Gen$2(0, (state_4) => {
                let kstate_3, lastMState_2, isStopped_2, lastKState_2, lastMState_1, x_1, xs;
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
                        const state_5 = new BindState$3(mstate_2, lastKState_1, empty(), isStopped_1);
                        return new Res$2(0, empty(), buildSkip(state_5));
                    }
                    else {
                        const activePatternResult853 = Gen_$007CLoopStateToOption$007C(lastMState, mres.fields[1]);
                        const mleftovers_1 = tail(mres.fields[0]);
                        const mstate_1 = activePatternResult853;
                        const mval_1 = head(mres.fields[0]);
                        return evalk_1(mval_1, mstate_1, mleftovers_1, lastKState_1, isStopped_1);
                    }
                };
                return (state_4 == null) ? evalmres(Gen_run(m_2)(void 0), void 0, void 0, false) : (isEmpty(state_4.mleftovers) ? (state_4.isStopped ? (new Res$2(1, empty())) : ((kstate_3 = state_4.kstate, (lastMState_2 = state_4.mstate, evalmres(Gen_run(m_2)(lastMState_2), lastMState_2, kstate_3, false))))) : ((isStopped_2 = state_4.isStopped, (lastKState_2 = state_4.kstate, (lastMState_1 = state_4.mstate, (x_1 = head(state_4.mleftovers), (xs = tail(state_4.mleftovers), evalk_1(x_1, lastMState_1, xs, lastKState_2, isStopped_2))))))));
            }))))))))(lastKState_3);
            if (matchValue_2.tag === 1) {
                const kvalues_3 = matchValue_2.fields[0];
                return new Res$2(1, kvalues_3);
            }
            else {
                const kvalues_2 = matchValue_2.fields[0];
                const kstate_4 = matchValue_2.fields[1];
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
                        return new Res$2(0, kvalues_2, new LoopState$1(0, newState_1(some(kstate_6))));
                    }
                }
            }
        };
        const buildSkip_2 = (state_6) => (new LoopState$1(0, state_6));
        const evalk_3 = evalk_2;
        const m_5 = Gen_initWith(() => App__NewSender(app()));
        return new Gen$2(0, (state_7) => {
            let kstate_7, lastMState_5, isStopped_5, lastKState_5, lastMState_4, x_2, xs_1;
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
                    const state_8 = new BindState$3(mstate_5, lastKState_4, empty(), isStopped_4);
                    return new Res$2(0, empty(), buildSkip_2(state_8));
                }
                else {
                    const activePatternResult853_1 = Gen_$007CLoopStateToOption$007C(lastMState_3, mres_1.fields[1]);
                    const mleftovers_4 = tail(mres_1.fields[0]);
                    const mstate_4 = activePatternResult853_1;
                    const mval_4 = head(mres_1.fields[0]);
                    return evalk_3(mval_4, mstate_4, mleftovers_4, lastKState_4, isStopped_4);
                }
            };
            return (state_7 == null) ? evalmres_1(Gen_run(m_5)(void 0), void 0, void 0, false) : (isEmpty(state_7.mleftovers) ? (state_7.isStopped ? (new Res$2(1, empty())) : ((kstate_7 = state_7.kstate, (lastMState_5 = state_7.mstate, evalmres_1(Gen_run(m_5)(lastMState_5), lastMState_5, kstate_7, false))))) : ((isStopped_5 = state_7.isStopped, (lastKState_5 = state_7.kstate, (lastMState_4 = state_7.mstate, (x_2 = head(state_7.mleftovers), (xs_1 = tail(state_7.mleftovers), evalk_3(x_2, lastMState_4, xs_1, lastKState_5, isStopped_5))))))));
        });
    })();
}

export function view() {
    return Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => {
        const comp = () => Gen_BaseBuilder__Delay_1505(TopLevelOperators_loop, () => {
            const evalk = (mval, mstate, mleftovers, lastKState, isStopped) => {
                let _arg1, setCount, count;
                const matchValue = Gen_run((_arg1 = mval, (setCount = _arg1[1], (count = (_arg1[0] | 0), Gen_BaseBuilder__ReturnFrom_1505(TopLevelOperators_loop, div([], button(text(toText(interpolate("Count = %P()", [count]))), () => {
                    setCount(count + 1);
                })))))))(lastKState);
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
            const m_2 = Gen_ofMutable(0);
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
        const evalk_6 = (mval_9, mstate_9, mleftovers_9, lastKState_9, isStopped_9) => {
            let c1, evalk_4, buildSkip_4, evalk_5, m_8;
            const matchValue_3 = Gen_run((c1 = mval_9, (evalk_4 = ((mval_6, mstate_6, mleftovers_6, lastKState_6, isStopped_6) => {
                let c2, evalk_2, buildSkip_2, evalk_3, m_5;
                const matchValue_2 = Gen_run((c2 = mval_6, (evalk_2 = ((mval_3, mstate_3, mleftovers_3, lastKState_3, isStopped_3) => {
                    let wrapper;
                    const matchValue_1 = Gen_run((wrapper = mval_3, ((wrapper.childNodes.length === 1) ? ((void wrapper.appendChild(c1), void wrapper.appendChild(c2))) : (void 0), Gen_LoopBuilder__Yield_1505(TopLevelOperators_loop, wrapper))))(lastKState_3);
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
                                return new Res$2(0, kvalues_2, new LoopState$1(0, newState_1(some(kstate_6))));
                            }
                        }
                    }
                }), (buildSkip_2 = ((state_3) => (new LoopState$1(0, state_3))), (evalk_3 = evalk_2, (m_5 = div([], Gen_initWith(() => document.createTextNode("---"))), new Gen$2(0, (state_4) => {
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
                })))))))(lastKState_6);
                if (matchValue_2.tag === 1) {
                    const kvalues_5 = matchValue_2.fields[0];
                    return new Res$2(1, kvalues_5);
                }
                else {
                    const kvalues_4 = matchValue_2.fields[0];
                    const kstate_8 = matchValue_2.fields[1];
                    const newState_2 = (kstate_9) => (new BindState$3(mstate_6, kstate_9, mleftovers_6, isStopped_6));
                    switch (kstate_8.tag) {
                        case 1: {
                            return new Res$2(0, kvalues_4, new LoopState$1(0, newState_2(lastKState_6)));
                        }
                        case 2: {
                            return new Res$2(0, kvalues_4, new LoopState$1(2));
                        }
                        default: {
                            const kstate_10 = kstate_8.fields[0];
                            return new Res$2(0, kvalues_4, new LoopState$1(0, newState_2(some(kstate_10))));
                        }
                    }
                }
            }), (buildSkip_4 = ((state_6) => (new LoopState$1(0, state_6))), (evalk_5 = evalk_4, (m_8 = comp(), new Gen$2(0, (state_7) => {
                let kstate_11, lastMState_8, isStopped_8, lastKState_8, lastMState_7, x_2, xs_2;
                const evalmres_2 = (mres_2, lastMState_6, lastKState_7, isStopped_7) => {
                    if (mres_2.tag === 1) {
                        if (isEmpty(mres_2.fields[0])) {
                            return new Res$2(1, empty());
                        }
                        else {
                            const mleftovers_8 = tail(mres_2.fields[0]);
                            const mval_8 = head(mres_2.fields[0]);
                            return evalk_5(mval_8, lastMState_6, mleftovers_8, lastKState_7, isStopped_7);
                        }
                    }
                    else if (isEmpty(mres_2.fields[0])) {
                        const activePatternResult854_2 = Gen_$007CLoopStateToOption$007C(lastMState_6, mres_2.fields[1]);
                        const mstate_8 = activePatternResult854_2;
                        const state_8 = new BindState$3(mstate_8, lastKState_7, empty(), isStopped_7);
                        return new Res$2(0, empty(), buildSkip_4(state_8));
                    }
                    else {
                        const activePatternResult853_2 = Gen_$007CLoopStateToOption$007C(lastMState_6, mres_2.fields[1]);
                        const mleftovers_7 = tail(mres_2.fields[0]);
                        const mstate_7 = activePatternResult853_2;
                        const mval_7 = head(mres_2.fields[0]);
                        return evalk_5(mval_7, mstate_7, mleftovers_7, lastKState_7, isStopped_7);
                    }
                };
                return (state_7 == null) ? evalmres_2(Gen_run(m_8)(void 0), void 0, void 0, false) : (isEmpty(state_7.mleftovers) ? (state_7.isStopped ? (new Res$2(1, empty())) : ((kstate_11 = state_7.kstate, (lastMState_8 = state_7.mstate, evalmres_2(Gen_run(m_8)(lastMState_8), lastMState_8, kstate_11, false))))) : ((isStopped_8 = state_7.isStopped, (lastKState_8 = state_7.kstate, (lastMState_7 = state_7.mstate, (x_2 = head(state_7.mleftovers), (xs_2 = tail(state_7.mleftovers), evalk_5(x_2, lastMState_7, xs_2, lastKState_8, isStopped_8))))))));
            })))))))(lastKState_9);
            if (matchValue_3.tag === 1) {
                const kvalues_7 = matchValue_3.fields[0];
                return new Res$2(1, kvalues_7);
            }
            else {
                const kvalues_6 = matchValue_3.fields[0];
                const kstate_12 = matchValue_3.fields[1];
                const newState_3 = (kstate_13) => (new BindState$3(mstate_9, kstate_13, mleftovers_9, isStopped_9));
                switch (kstate_12.tag) {
                    case 1: {
                        return new Res$2(0, kvalues_6, new LoopState$1(0, newState_3(lastKState_9)));
                    }
                    case 2: {
                        return new Res$2(0, kvalues_6, new LoopState$1(2));
                    }
                    default: {
                        const kstate_14 = kstate_12.fields[0];
                        return new Res$2(0, kvalues_6, new LoopState$1(0, newState_3(some(kstate_14))));
                    }
                }
            }
        };
        const buildSkip_6 = (state_9) => (new LoopState$1(0, state_9));
        const evalk_7 = evalk_6;
        const m_11 = comp();
        return new Gen$2(0, (state_10) => {
            let kstate_15, lastMState_11, isStopped_11, lastKState_11, lastMState_10, x_3, xs_3;
            const evalmres_3 = (mres_3, lastMState_9, lastKState_10, isStopped_10) => {
                if (mres_3.tag === 1) {
                    if (isEmpty(mres_3.fields[0])) {
                        return new Res$2(1, empty());
                    }
                    else {
                        const mleftovers_11 = tail(mres_3.fields[0]);
                        const mval_11 = head(mres_3.fields[0]);
                        return evalk_7(mval_11, lastMState_9, mleftovers_11, lastKState_10, isStopped_10);
                    }
                }
                else if (isEmpty(mres_3.fields[0])) {
                    const activePatternResult854_3 = Gen_$007CLoopStateToOption$007C(lastMState_9, mres_3.fields[1]);
                    const mstate_11 = activePatternResult854_3;
                    const state_11 = new BindState$3(mstate_11, lastKState_10, empty(), isStopped_10);
                    return new Res$2(0, empty(), buildSkip_6(state_11));
                }
                else {
                    const activePatternResult853_3 = Gen_$007CLoopStateToOption$007C(lastMState_9, mres_3.fields[1]);
                    const mleftovers_10 = tail(mres_3.fields[0]);
                    const mstate_10 = activePatternResult853_3;
                    const mval_10 = head(mres_3.fields[0]);
                    return evalk_7(mval_10, mstate_10, mleftovers_10, lastKState_10, isStopped_10);
                }
            };
            return (state_10 == null) ? evalmres_3(Gen_run(m_11)(void 0), void 0, void 0, false) : (isEmpty(state_10.mleftovers) ? (state_10.isStopped ? (new Res$2(1, empty())) : ((kstate_15 = state_10.kstate, (lastMState_11 = state_10.mstate, evalmres_3(Gen_run(m_11)(lastMState_11), lastMState_11, kstate_15, false))))) : ((isStopped_11 = state_10.isStopped, (lastKState_11 = state_10.kstate, (lastMState_10 = state_10.mstate, (x_3 = head(state_10.mleftovers), (xs_3 = tail(state_10.mleftovers), evalk_7(x_3, lastMState_10, xs_3, lastKState_11, isStopped_11))))))));
        });
    })();
}

(function () {
    const evaluableView = Gen_toEvaluable(view());
    app(App_$ctor_3C4DE8D7(document.querySelector("#app"), (_arg1) => Gen_Evaluable$1__Evaluate(evaluableView)), true);
    App__Run(app());
})();

//# sourceMappingURL=app.js.map
