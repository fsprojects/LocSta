import { Union } from "./.fable/fable-library.3.2.9/Types.js";
import { class_type, union_type, int32_type } from "./.fable/fable-library.3.2.9/Reflection.js";
import { toText, printf, interpolate, toConsole } from "./.fable/fable-library.3.2.9/String.js";
import { contains, map, delay } from "./.fable/fable-library.3.2.9/Seq.js";
import { rangeDouble } from "./.fable/fable-library.3.2.9/Range.js";
import { Gen_GenBuilder__ReturnFrom_Z781C29E4, Gen_ofMutable, Gen_loop, Gen_GenBuilder__Return_1505, Gen_preserve, Gen_GenBuilder__Bind_456328F3 } from "./coreOld.js";
import { structuralHash, equals, getEnumerator, safeHash, uncurry } from "./.fable/fable-library.3.2.9/Util.js";
import { ofArray } from "./.fable/fable-library.3.2.9/List.js";

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

export function app(s, r) {
    return [r, void 0];
}

export function Browser_Types_NodeList__NodeList_get_Seq(this$) {
    return delay(() => map((i) => (this$[i]), rangeDouble(0, 1, this$.length - 1)));
}

export function elem(name, attributes, child) {
    return Gen_GenBuilder__Bind_456328F3(Gen_loop, (s, r) => app(s, r), uncurry(3, (_arg1) => {
        const app_1 = _arg1;
        return Gen_GenBuilder__Bind_456328F3(Gen_loop, (s_1, r_1) => Gen_preserve(() => App__CreateElement_Z721C83C5(app_1, name), s_1, r_1), uncurry(3, (_arg2) => {
            const elem_1 = _arg2;
            toConsole(interpolate("Eval: %P() (%P())", [name, safeHash(elem_1)]));
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
                        const elemAttr = elem_1.attributes.getNamedItem(aname);
                        if (elemAttr.value !== avalue) {
                            elemAttr.value = avalue;
                        }
                    }
                }
                finally {
                    enumerator.Dispose();
                }
                if (!contains(child_1, Browser_Types_NodeList__NodeList_get_Seq(elem_1.childNodes), {
                    Equals: (x_3, y) => equals(x_3, y),
                    GetHashCode: (x_3) => structuralHash(x_3),
                })) {
                    toConsole(interpolate("add child (node count = %P())", [elem_1.childNodes.length]));
                    void elem_1.appendChild(child_1);
                }
                return Gen_GenBuilder__Return_1505(Gen_loop, elem_1);
            }));
        }));
    }));
}

export function text(content) {
    return Gen_GenBuilder__Bind_456328F3(Gen_loop, (s, r) => Gen_preserve(() => document.createTextNode(content), s, r), uncurry(3, (_arg1) => {
        const elem_1 = _arg1;
        if (elem_1.textContent !== content) {
            elem_1.textContent = content;
        }
        return Gen_GenBuilder__Return_1505(Gen_loop, elem_1);
    }));
}

export function div(attributes, content) {
    return elem("div", attributes, content);
}

export function p(attributes, content) {
    return elem("p", attributes, content);
}

export function button(content, click) {
    return Gen_GenBuilder__Bind_456328F3(Gen_loop, (s, r) => app(s, r), uncurry(3, (_arg1) => {
        let clo3;
        const app_1 = _arg1;
        return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, (clo3 = elem("button", [], content), (arg30) => {
            const clo4 = clo3(arg30);
            return (arg40) => clo4(arg40);
        })), uncurry(3, (_arg1_1) => {
            const x$0027 = _arg1_1;
            return Gen_GenBuilder__Return_1505(Gen_loop, x$0027);
        }))), uncurry(3, (_arg2) => {
            const button_1 = _arg2;
            button_1.onclick = ((_arg1_2) => {
                toConsole(printf("-----CLICK"));
                click();
                App__TriggerUpdate_B088534(app_1, void 0);
            });
            return Gen_GenBuilder__Return_1505(Gen_loop, button_1);
        }));
    }));
}

export function view() {
    const comp = Gen_GenBuilder__Bind_456328F3(Gen_loop, (s, r) => Gen_ofMutable(0, s, r), uncurry(3, (_arg1) => {
        let clo2;
        const setCount = _arg1[1];
        const count = _arg1[0] | 0;
        return Gen_GenBuilder__ReturnFrom_Z781C29E4(Gen_loop, uncurry(2, (clo2 = div([], uncurry(2, button(uncurry(2, text(toText(interpolate("Count = %P()", [count])))), () => {
            setCount(count + 1);
        }))), (arg20) => {
            const clo3 = clo2(arg20);
            return (arg30) => clo3(arg30);
        })));
    }));
    const x_7 = ofArray([Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, comp), uncurry(3, (_arg1_1) => {
        const x$0027 = _arg1_1;
        return Gen_GenBuilder__Return_1505(Gen_loop, x$0027);
    })), Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, button(uncurry(2, comp), () => {
    })), uncurry(3, (_arg1_2) => {
        const x$0027_1 = _arg1_2;
        return Gen_GenBuilder__Return_1505(Gen_loop, x$0027_1);
    }))]);
    return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, comp), uncurry(3, (_arg2) => {
        const c1 = _arg2;
        return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, comp), uncurry(3, (_arg3) => {
            let clo2_1;
            const c2 = _arg3;
            return Gen_GenBuilder__Bind_456328F3(Gen_loop, uncurry(2, (clo2_1 = div([], (s_1, r_1) => Gen_preserve(() => document.createTextNode("---"), s_1, r_1)), (arg20_1) => {
                const clo3_1 = clo2_1(arg20_1);
                return (arg30_1) => clo3_1(arg30_1);
            })), uncurry(3, (_arg4) => {
                const wrapper = _arg4;
                if (wrapper.childNodes.length === 1) {
                    void wrapper.appendChild(c1);
                    void wrapper.appendChild(c2);
                }
                return Gen_GenBuilder__Return_1505(Gen_loop, wrapper);
            }));
        }));
    }));
}

App__Run(App_$ctor_Z7B97B9E3(document.querySelector("#app"), (() => {
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
