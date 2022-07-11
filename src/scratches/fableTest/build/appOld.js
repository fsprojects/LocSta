import { FSharpRef, Union } from "./fable_modules/fable-library.3.7.16/Types.js";
import { record_type, array_type, obj_type, tuple_type, unit_type, equals, class_type, union_type, int32_type } from "./fable_modules/fable-library.3.7.16/Reflection.js";
import { printf, toConsole } from "./fable_modules/fable-library.3.7.16/String.js";
import { singleton, tryFindIndex, collect, map, delay } from "./fable_modules/fable-library.3.7.16/Seq.js";
import { rangeDouble } from "./fable_modules/fable-library.3.7.16/Range.js";
import { singleton as singleton_1, empty, append } from "./fable_modules/fable-library.3.7.16/List.js";
import { curry, uncurry, disposeSafe, getEnumerator, safeHash, partialApply } from "./fable_modules/fable-library.3.7.16/Util.js";
import { defaultArgWith, some } from "./fable_modules/fable-library.3.7.16/Option.js";
import { Gen_GenBuilder__ReturnFrom_Z781C29E4, Gen_ofMutable, Gen_loop, Gen_GenBuilder__Return_1505, Gen_preserve } from "./coreOld.js";

//# sourceMappingURL=appOld.js.map
export class Application_Sender extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Id"];
    }
}

//# sourceMappingURL=appOld.js.map
export function Application_Sender$reflection() {
    return union_type("App.Application.Sender", [], Application_Sender, () => [[["Item", int32_type]]]);
}

//# sourceMappingURL=appOld.js.map
export class Application_App {
    constructor(document$, appElement, triggerUpdate) {
        this.document = document$;
        this.appElement = appElement;
        this.triggerUpdate = triggerUpdate;
        this.currId = -1;
        this["CurrentSender@"] = (void 0);
    }
}

//# sourceMappingURL=appOld.js.map
export function Application_App$reflection() {
    return class_type("App.Application.App", void 0, Application_App);
}

//# sourceMappingURL=appOld.js.map
export function Application_App_$ctor_1838A887(document$, appElement, triggerUpdate) {
    return new Application_App(document$, appElement, triggerUpdate);
}

//# sourceMappingURL=appOld.js.map
export function Application_App__get_CurrentSender(__) {
    return __["CurrentSender@"];
}

//# sourceMappingURL=appOld.js.map
export function Application_App__set_CurrentSender_6187BDC0(__, v) {
    __["CurrentSender@"] = v;
}

//# sourceMappingURL=appOld.js.map
export function Application_App__NewSender(_) {
    _.currId = ((_.currId + 1) | 0);
    toConsole(`New sender: ${_.currId}`);
    return new Application_Sender(0, _.currId);
}

//# sourceMappingURL=appOld.js.map
export function Application_App__get_Document(_) {
    return _.document;
}

//# sourceMappingURL=appOld.js.map
export function Application_App__Run(this$) {
    const initialElement = this$.triggerUpdate(this$);
    this$.appElement.appendChild(initialElement);
}

//# sourceMappingURL=appOld.js.map
export function Application_App__TriggerUpdate_6187BDC0(this$, sender) {
    Application_App__set_CurrentSender_6187BDC0(this$, sender);
    toConsole(`Trigger update with sender: ${sender}`);
    const element = this$.triggerUpdate(this$);
}

//# sourceMappingURL=appOld.js.map
export function Application_app(s, r) {
    return [r, void 0];
}

//# sourceMappingURL=appOld.js.map
export function Browser_Types_NodeList__NodeList_get_elements(this$) {
    return delay(() => map((i) => (this$[i]), rangeDouble(0, 1, this$.length - 1)));
}

//# sourceMappingURL=appOld.js.map
export function Browser_Types_Node__Node_clearChildren(this$) {
    this$.textContent = "";
}

//# sourceMappingURL=appOld.js.map
export class Framework_ChildrenBuilder$1 {
    constructor(run) {
        this.run = run;
    }
}

//# sourceMappingURL=appOld.js.map
export function Framework_ChildrenBuilder$1$reflection(gen0) {
    return class_type("App.Framework.ChildrenBuilder`1", [gen0], Framework_ChildrenBuilder$1);
}

//# sourceMappingURL=appOld.js.map
export function Framework_ChildrenBuilder$1_$ctor_4F2D3AE5(run) {
    return new Framework_ChildrenBuilder$1(run);
}

//# sourceMappingURL=appOld.js.map
export function Framework_ChildrenBuilder$1__Combine_Z12967EE0(_, a, b) {
    return append(a, b);
}

//# sourceMappingURL=appOld.js.map
export function Framework_ChildrenBuilder$1__Zero(_) {
    return empty();
}

//# sourceMappingURL=appOld.js.map
export function Framework_ChildrenBuilder$1__Run_24C1E7D4(_, children) {
    return partialApply(2, _.run, [children]);
}

//# sourceMappingURL=appOld.js.map
export function Framework_elem(name, attributes, children) {
    console.log(some(children));
    const syncChildren = (elem, s, r) => {
        const s_1 = defaultArgWith(s, () => []);
        const newState = delay(() => collect((matchValue) => {
            const childType = matchValue[0];
            const childGen = matchValue[1];
            const stateIdx = tryFindIndex((tupledArg) => {
                const typ = tupledArg[0];
                return equals(typ, childType);
            }, s_1);
            let newChildState;
            if (stateIdx == null) {
                const patternInput = childGen(void 0)(r);
                const s_2 = patternInput[1];
                const o = patternInput[0];
                elem.appendChild(o);
                newChildState = s_2;
            }
            else {
                const idx = stateIdx | 0;
                const childState = s_1[idx];
                s_1.splice(idx, 1);
                newChildState = childGen(some(childState[1]))(r)[1];
            }
            return singleton([childType, newChildState]);
        }, children));
        return [void 0, Array.from(newState)];
    };
    return (mfState_4) => ((r_7) => {
        const mfState_5 = mfState_4;
        const r_8 = r_7;
        let patternInput_4;
        if (mfState_5 != null) {
            const mState_3 = mfState_5[0];
            const fState_3 = mfState_5[1];
            patternInput_4 = [some(mState_3), fState_3];
        }
        else {
            patternInput_4 = [void 0, void 0];
        }
        const mState_1_2 = patternInput_4[0];
        const fState_1_2 = patternInput_4[1];
        const patternInput_1_3 = Application_app(mState_1_2, r_8);
        const mState$0027_2 = patternInput_1_3[1];
        const mOut_2 = patternInput_1_3[0];
        let fgen_2;
        const app = mOut_2;
        fgen_2 = ((mfState_2) => ((r_5) => {
            const mfState_3 = mfState_2;
            const r_6 = r_5;
            let patternInput_3;
            if (mfState_3 != null) {
                const mState_2 = mfState_3[0];
                const fState_2 = mfState_3[1];
                patternInput_3 = [mState_2, fState_2];
            }
            else {
                patternInput_3 = [void 0, void 0];
            }
            const mState_1_1 = patternInput_3[0];
            const fState_1_1 = patternInput_3[1];
            const patternInput_1_2 = Gen_preserve(() => Application_App__get_Document(app).createElement(name), mState_1_1, r_6);
            const mState$0027_1 = patternInput_1_2[1];
            const mOut_1 = patternInput_1_2[0];
            let fgen_1;
            const elem_1 = mOut_1;
            toConsole(`Eval: ${name} (${safeHash(elem_1)})`);
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
                disposeSafe(enumerator);
            }
            const m = partialApply(2, syncChildren, [elem_1]);
            fgen_1 = ((mfState) => ((r_3) => {
                const mfState_1 = mfState;
                const r_4 = r_3;
                let patternInput_1;
                if (mfState_1 != null) {
                    const mState = mfState_1[0];
                    const fState = mfState_1[1];
                    patternInput_1 = [mState, some(fState)];
                }
                else {
                    patternInput_1 = [void 0, void 0];
                }
                const mState_1 = patternInput_1[0];
                const fState_1 = patternInput_1[1];
                const patternInput_1_1 = m(mState_1)(r_4);
                const mState$0027 = patternInput_1_1[1];
                const mOut = patternInput_1_1[0];
                const fgen = Gen_GenBuilder__Return_1505(Gen_loop, elem_1);
                const patternInput_2 = fgen(fState_1)(r_4);
                const fState$0027 = patternInput_2[1];
                const fOut = patternInput_2[0];
                const resultingState = [mState$0027, fState$0027];
                return [fOut, resultingState];
            }));
            const patternInput_2_1 = fgen_1(fState_1_1)(r_6);
            const fState$0027_1 = patternInput_2_1[1];
            const fOut_1 = patternInput_2_1[0];
            const resultingState_1 = [mState$0027_1, fState$0027_1];
            return [fOut_1, resultingState_1];
        }));
        const patternInput_2_2 = fgen_2(fState_1_2)(r_8);
        const fState$0027_2 = patternInput_2_2[1];
        const fOut_2 = patternInput_2_2[0];
        const resultingState_2 = [mState$0027_2, fState$0027_2];
        return [fOut_2, resultingState_2];
    });
}

//# sourceMappingURL=appOld.js.map
export function HtmlElementsApi_text(text) {
    return (mfState_2) => ((r_4) => {
        const mfState_3 = mfState_2;
        const r_5 = r_4;
        let patternInput_3;
        if (mfState_3 != null) {
            const mState_2 = mfState_3[0];
            const fState_2 = mfState_3[1];
            patternInput_3 = [some(mState_2), fState_2];
        }
        else {
            patternInput_3 = [void 0, void 0];
        }
        const mState_1_1 = patternInput_3[0];
        const fState_1_1 = patternInput_3[1];
        const patternInput_1_1 = Application_app(mState_1_1, r_5);
        const mState$0027_1 = patternInput_1_1[1];
        const mOut_1 = patternInput_1_1[0];
        let fgen_1;
        const app = mOut_1;
        fgen_1 = ((mfState) => ((r_2) => {
            const mfState_1 = mfState;
            const r_3 = r_2;
            let patternInput;
            if (mfState_1 != null) {
                const mState = mfState_1[0];
                const fState = mfState_1[1];
                patternInput = [mState, some(fState)];
            }
            else {
                patternInput = [void 0, void 0];
            }
            const mState_1 = patternInput[0];
            const fState_1 = patternInput[1];
            const patternInput_1 = Gen_preserve(() => Application_App__get_Document(app).createTextNode(text), mState_1, r_3);
            const mState$0027 = patternInput_1[1];
            const mOut = patternInput_1[0];
            let fgen;
            const elem = mOut;
            if (elem.textContent !== text) {
                elem.textContent = text;
            }
            fgen = Gen_GenBuilder__Return_1505(Gen_loop, elem);
            const patternInput_2 = fgen(fState_1)(r_3);
            const fState$0027 = patternInput_2[1];
            const fOut = patternInput_2[0];
            const resultingState = [mState$0027, fState$0027];
            return [fOut, resultingState];
        }));
        const patternInput_2_1 = fgen_1(fState_1_1)(r_5);
        const fState$0027_1 = patternInput_2_1[1];
        const fOut_1 = patternInput_2_1[0];
        const resultingState_1 = [mState$0027_1, fState$0027_1];
        return [fOut_1, resultingState_1];
    });
}

//# sourceMappingURL=appOld.js.map
export function HtmlElementsApi_div(attributes) {
    return Framework_ChildrenBuilder$1_$ctor_4F2D3AE5(uncurry(3, (children) => Framework_elem("div", attributes, children)));
}

//# sourceMappingURL=appOld.js.map
export function HtmlElementsApi_p(attributes) {
    return Framework_ChildrenBuilder$1_$ctor_4F2D3AE5(uncurry(3, (children) => Framework_elem("p", attributes, children)));
}

//# sourceMappingURL=appOld.js.map
export function HtmlElementsApi_button(attributes, click) {
    return Framework_ChildrenBuilder$1_$ctor_4F2D3AE5((children, mfState_2, r_5) => {
        const mfState_3 = mfState_2;
        const r_6 = r_5;
        let patternInput_3;
        if (mfState_3 != null) {
            const mState_2 = mfState_3[0];
            const fState_2 = mfState_3[1];
            patternInput_3 = [some(mState_2), fState_2];
        }
        else {
            patternInput_3 = [void 0, void 0];
        }
        const mState_1_1 = patternInput_3[0];
        const fState_1_1 = patternInput_3[1];
        const patternInput_1_2 = Application_app(mState_1_1, r_6);
        const mState$0027_1 = patternInput_1_2[1];
        const mOut_1 = patternInput_1_2[0];
        let fgen_1;
        const app = mOut_1;
        let m;
        const g = Framework_elem("button", attributes, children);
        m = ((s_1) => ((r_1) => {
            const patternInput = g(s_1)(r_1);
            const s_1_1 = patternInput[1];
            const o = patternInput[0];
            return [o, s_1_1];
        }));
        fgen_1 = ((mfState) => ((r_3) => {
            const mfState_1 = mfState;
            const r_4 = r_3;
            let patternInput_1;
            if (mfState_1 != null) {
                const mState = mfState_1[0];
                const fState = mfState_1[1];
                patternInput_1 = [mState, some(fState)];
            }
            else {
                patternInput_1 = [void 0, void 0];
            }
            const mState_1 = patternInput_1[0];
            const fState_1 = patternInput_1[1];
            const patternInput_1_1 = m(mState_1)(r_4);
            const mState$0027 = patternInput_1_1[1];
            const mOut = patternInput_1_1[0];
            let fgen;
            const button = mOut;
            button.onclick = ((_arg_2) => {
                toConsole(printf("-----CLICK"));
                click();
                Application_App__TriggerUpdate_6187BDC0(app, void 0);
            });
            fgen = Gen_GenBuilder__Return_1505(Gen_loop, button);
            const patternInput_2 = fgen(fState_1)(r_4);
            const fState$0027 = patternInput_2[1];
            const fOut = patternInput_2[0];
            const resultingState = [mState$0027, fState$0027];
            return [fOut, resultingState];
        }));
        const patternInput_2_1 = fgen_1(fState_1_1)(r_6);
        const fState$0027_1 = patternInput_2_1[1];
        const fOut_1 = patternInput_2_1[0];
        const resultingState_1 = [mState$0027_1, fState$0027_1];
        return [fOut_1, resultingState_1];
    });
}

//# sourceMappingURL=appOld.js.map
export const comp = (mfState) => ((r_3) => {
    let x_1, x, g_1, g_1_1;
    const mfState_1 = mfState;
    const r_4 = r_3;
    let patternInput_2;
    if (mfState_1 != null) {
        const mState = mfState_1[0];
        const fState = mfState_1[1];
        patternInput_2 = [mState, fState];
    }
    else {
        patternInput_2 = [void 0, void 0];
    }
    const mState_1 = patternInput_2[0];
    const fState_1 = patternInput_2[1];
    const patternInput_1_1 = Gen_ofMutable(0, mState_1, r_4);
    const mState$0027 = patternInput_1_1[1];
    const mOut = patternInput_1_1[0];
    let fgen;
    const _arg = mOut;
    const setCount = _arg[1];
    const count = _arg[0] | 0;
    fgen = Gen_GenBuilder__ReturnFrom_Z781C29E4(Gen_loop, uncurry(2, Framework_ChildrenBuilder$1__Run_24C1E7D4(HtmlElementsApi_div([]), (x_1 = Framework_ChildrenBuilder$1__Run_24C1E7D4(HtmlElementsApi_button([], () => {
        setCount(count + 1);
    }), (x = HtmlElementsApi_text(`Count = ${count}`), singleton_1((g_1 = ((s_1, r_1) => {
        const patternInput = x(s_1)(r_1);
        const s_1_1 = patternInput[1];
        const o = patternInput[0];
        return [o, s_1_1];
    }), [tuple_type(unit_type, tuple_type(class_type("Browser.Types.Text"), unit_type)), curry(2, g_1)])))), singleton_1((g_1_1 = ((s_2, r_2) => {
        const patternInput_1 = x_1(s_2)(r_2);
        const s_1_2 = patternInput_1[1];
        const o_1 = patternInput_1[0];
        return [o_1, s_1_2];
    }), [tuple_type(unit_type, tuple_type(tuple_type(unit_type, tuple_type(class_type("Browser.Types.Node"), tuple_type(array_type(tuple_type(class_type("System.Type"), obj_type)), unit_type))), unit_type)), curry(2, g_1_1)]))))));
    const patternInput_2_1 = fgen(fState_1)(r_4);
    const fState$0027 = patternInput_2_1[1];
    const fOut = patternInput_2_1[0];
    const resultingState = [mState$0027, fState$0027];
    return [fOut, resultingState];
});

//# sourceMappingURL=appOld.js.map
export function view() {
    let g_1, x_3, builder$0040_1, x_1, g_1_1, g_1_2, g_1_3;
    const builder$0040 = HtmlElementsApi_div([]);
    return Framework_ChildrenBuilder$1__Run_24C1E7D4(builder$0040, Framework_ChildrenBuilder$1__Combine_Z12967EE0(builder$0040, singleton_1((g_1 = ((s, r) => {
        const patternInput = comp(s)(r);
        const s_1 = patternInput[1];
        const o = patternInput[0];
        return [o, s_1];
    }), [tuple_type(record_type("Microsoft.FSharp.Core.FSharpRef`1", [int32_type], FSharpRef, () => [["contents", int32_type]]), tuple_type(unit_type, tuple_type(class_type("Browser.Types.Node"), tuple_type(array_type(tuple_type(class_type("System.Type"), obj_type)), unit_type)))), curry(2, g_1)])), (x_3 = ((builder$0040_1 = HtmlElementsApi_div([]), Framework_ChildrenBuilder$1__Run_24C1E7D4(builder$0040_1, Framework_ChildrenBuilder$1__Combine_Z12967EE0(builder$0040_1, (x_1 = HtmlElementsApi_text("Hurz"), singleton_1((g_1_1 = ((s_2, r_1) => {
        const patternInput_1 = x_1(s_2)(r_1);
        const s_1_1 = patternInput_1[1];
        const o_1 = patternInput_1[0];
        return [o_1, s_1_1];
    }), [tuple_type(unit_type, tuple_type(class_type("Browser.Types.Text"), unit_type)), curry(2, g_1_1)]))), singleton_1((g_1_2 = ((s_3, r_2) => {
        const patternInput_2 = comp(s_3)(r_2);
        const s_1_2 = patternInput_2[1];
        const o_2 = patternInput_2[0];
        return [o_2, s_1_2];
    }), [tuple_type(record_type("Microsoft.FSharp.Core.FSharpRef`1", [int32_type], FSharpRef, () => [["contents", int32_type]]), tuple_type(unit_type, tuple_type(class_type("Browser.Types.Node"), tuple_type(array_type(tuple_type(class_type("System.Type"), obj_type)), unit_type)))), curry(2, g_1_2)])))))), singleton_1((g_1_3 = ((s_4, r_3) => {
        const patternInput_3 = x_3(s_4)(r_3);
        const s_1_3 = patternInput_3[1];
        const o_3 = patternInput_3[0];
        return [o_3, s_1_3];
    }), [tuple_type(unit_type, tuple_type(class_type("Browser.Types.Node"), tuple_type(array_type(tuple_type(class_type("System.Type"), obj_type)), unit_type))), curry(2, g_1_3)])))));
}

//# sourceMappingURL=appOld.js.map
Application_App__Run(Application_App_$ctor_1838A887(document, document.querySelector("#app"), (() => {
    const g = view();
    let state = void 0;
    return (r) => {
        const patternInput = g(state)(r);
        const fState = patternInput[1];
        const fOut = patternInput[0];
        state = fState;
        return fOut;
    };
})()));

//# sourceMappingURL=appOld.js.map
