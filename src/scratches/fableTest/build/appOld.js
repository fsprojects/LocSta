import { Union } from "./.fable/fable-library.3.2.9/Types.js";
import { class_type, union_type, int32_type } from "./.fable/fable-library.3.2.9/Reflection.js";
import { toText, printf, interpolate, toConsole } from "./.fable/fable-library.3.2.9/String.js";
import { contains, map, delay } from "./.fable/fable-library.3.2.9/Seq.js";
import { rangeDouble } from "./.fable/fable-library.3.2.9/Range.js";
import { Gen_GenBuilder__ReturnFrom_Z781C29E4, Gen_ofMutable, Gen_loop, Gen_GenBuilder__Return_1505, Gen_preserve, Gen_GenBuilder__Bind_456328F3 } from "./coreOld.js";
import { structuralHash, equals, getEnumerator, safeHash, uncurry } from "./.fable/fable-library.3.2.9/Util.js";

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

export function App_$ctor_Z7B97B9E3(appElement, triggerUpdate) {
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

export function App__Run(this$) {
    const initialElement = this$.triggerUpdate(this$);
    void this$.appElement.appendChild(initialElement);
}

export function App__TriggerUpdate_B088534(this$, sender) {
    App__set_CurrentSender_B088534(this$, sender);
    toConsole(interpolate("Trigger update with sender: %P()", [sender]));
    const element = this$.triggerUpdate(this$);
}

export function getApp(unitVar0, s, r) {
    return [r, void 0];
}

export function Browser_Types_NodeList__NodeList_get_Seq(this$) {
    return delay(() => map((i) => (this$[i]), rangeDouble(0, 1, this$.length - 1)));
}

export function genList(children, s, r) {
    let g, state;
    const elem_1 = (name) => ((attributes) => ((child) => Gen_GenBuilder__Bind_456328F3(Gen_loop, (s_1, r_1) => getApp(void 0, s_1, r_1), uncurry(3, (_arg1) => {
        const app = _arg1;
        return Gen_GenBuilder__Bind_456328F3(Gen_loop, (s_2, r_2) => Gen_preserve(() => App__CreateElement_Z721C83C5(app, name), s_2, r_2), uncurry(3, (_arg2) => {
            const elem = _arg2;
            toConsole(interpolate("Eval: %P() (%P())", [name, safeHash(elem)]));
            return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, Gen_GenBuilder__Bind_456328F3(Gen_loop, child, uncurry(3, (_arg1_1) => {
                const x$0027 = _arg1_1;
                return Gen_GenBuilder__Return_1505(Gen_loop, x$0027);
            }))), uncurry(3, (_arg3) => {
                const child_1 = _arg3;
                const enumerator = getEnumerator(attributes);
                try {
                    while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                        const forLoopVar = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                        const avalue = forLoopVar[1];
                        const aname = forLoopVar[0];
                        const elemAttr = elem.attributes.getNamedItem(aname);
                        if (elemAttr.value !== avalue) {
                            elemAttr.value = avalue;
                        }
                    }
                }
                finally {
                    enumerator.Dispose();
                }
                if (!contains(child_1, Browser_Types_NodeList__NodeList_get_Seq(elem.childNodes), {
                    Equals: (x_3, y) => equals(x_3, y),
                    GetHashCode: (x_3) => structuralHash(x_3),
                })) {
                    toConsole(interpolate("add child (node count = %P())", [elem.childNodes.length]));
                    void elem.appendChild(child_1);
                }
                return Gen_GenBuilder__Return_1505(Gen_loop, elem);
            }));
        }));
    }))));
    const text = (content) => Gen_GenBuilder__Bind_456328F3(Gen_loop, (s_3, r_3) => Gen_preserve(() => document.createTextNode(content), s_3, r_3), uncurry(3, (_arg4) => {
        const elem_2 = _arg4;
        if (elem_2.textContent !== content) {
            elem_2.textContent = content;
        }
        return Gen_GenBuilder__Return_1505(Gen_loop, elem_2);
    }));
    const div = (attributes_1) => ((content_1) => elem_1("div")(attributes_1)(content_1));
    const p = (attributes_2) => ((content_2) => elem_1("p")(attributes_2)(content_2));
    const button_1 = (content_3) => ((click) => Gen_GenBuilder__Bind_456328F3(Gen_loop, (s_4, r_4) => getApp(void 0, s_4, r_4), uncurry(3, (_arg5) => {
        let clo3;
        const app_1 = _arg5;
        return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, (clo3 = elem_1("button")([])(content_3), (arg30) => {
            const clo4 = clo3(arg30);
            return (arg40) => clo4(arg40);
        })), uncurry(3, (_arg1_2) => {
            const x$0027_1 = _arg1_2;
            return Gen_GenBuilder__Return_1505(Gen_loop, x$0027_1);
        }))), uncurry(3, (_arg6) => {
            const button = _arg6;
            button.onclick = ((_arg1_3) => {
                toConsole(printf("-----CLICK"));
                click();
                App__TriggerUpdate_B088534(app_1, void 0);
            });
            return Gen_GenBuilder__Return_1505(Gen_loop, button);
        }));
    })));
    const view = () => {
        const comp = () => Gen_GenBuilder__Bind_456328F3(Gen_loop, (s_5, r_5) => Gen_ofMutable(0, s_5, r_5), uncurry(3, (_arg7) => {
            let arg10_1, clo2_1;
            const setCount = _arg7[1];
            const count = _arg7[0] | 0;
            return Gen_GenBuilder__ReturnFrom_Z781C29E4(Gen_loop, uncurry(2, (arg10_1 = button_1(uncurry(2, text(toText(interpolate("Count = %P()", [count])))))(() => {
                setCount(count + 1);
            }), (clo2_1 = div([])(uncurry(2, arg10_1)), (arg20_1) => {
                const clo3_1 = clo2_1(arg20_1);
                return (arg30_1) => clo3_1(arg30_1);
            }))));
        }));
        return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, comp()), uncurry(3, (_arg8) => {
            const c1 = _arg8;
            return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, comp()), uncurry(3, (_arg9) => {
                let clo2_2;
                const c2 = _arg9;
                return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, (clo2_2 = div([])((s_6, r_6) => Gen_preserve(() => document.createTextNode("---"), s_6, r_6)), (arg20_2) => {
                    const clo3_2 = clo2_2(arg20_2);
                    return (arg30_2) => clo3_2(arg30_2);
                })), uncurry(3, (_arg10) => {
                    const wrapper = _arg10;
                    if (wrapper.childNodes.length === 1) {
                        void wrapper.appendChild(c1);
                        void wrapper.appendChild(c2);
                    }
                    return Gen_GenBuilder__Return_1505(Gen_loop, wrapper);
                }));
            }));
        }));
    };
    App__Run(App_$ctor_Z7B97B9E3(document.querySelector("#app"), (g = view(), (state = (void 0), (r_7) => {
        const patternInput = g(state)(r_7);
        const fState = patternInput[1];
        const fOut = patternInput[0];
        state = fState;
        return fOut;
    }))));
}

//# sourceMappingURL=appOld.js.map
