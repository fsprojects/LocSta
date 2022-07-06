import { Gen$2, LoopState$1, BindState$3, Res$2, TopLevelOperators_feed, Gen_FeedBuilder__Yield_2A0A0, Gen_run, Init$1, Gen_BaseBuilder__Delay_1505 } from "./core.fs.js";
import { value, some } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/Option.js";
import { empty } from "../scratches/fableTest/src/.fable/fable-library.3.2.9/List.js";

function dotnetRandom() {
    return {};
}

export function random() {
    return Gen_BaseBuilder__Delay_1505(TopLevelOperators_feed, () => {
        const m_1 = new Init$1(0, dotnetRandom());
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
                let random_1, tupledArg;
                const matchValue = Gen_run((random_1 = lastFeed, (tupledArg = [Math.random(), random_1], Gen_FeedBuilder__Yield_2A0A0(TopLevelOperators_feed, tupledArg[0], tupledArg[1]))))(lastKState);
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
            return (state == null) ? evalk(getInitial(), void 0) : (state.isStopped ? (new Res$2(1, empty())) : ((state.mstate != null) ? ((feedback_4 = value(state.mstate), (kstate_3 = state.kstate, evalk(feedback_4, kstate_3)))) : ((kstate_2 = state.kstate, evalk(getInitial(), kstate_2)))));
        });
    })();
}

