import { class_type } from "./fable_modules/fable-library.3.7.16/Reflection.js";
import { curry } from "./fable_modules/fable-library.3.7.16/Util.js";
import { defaultArgWith } from "./fable_modules/fable-library.3.7.16/Option.js";
import { FSharpRef } from "./fable_modules/fable-library.3.7.16/Types.js";

//# sourceMappingURL=coreOld.js.map
export class Gen_GenBuilder {
    constructor() {
    }
}

//# sourceMappingURL=coreOld.js.map
export function Gen_GenBuilder$reflection() {
    return class_type("LocSta.Gen.GenBuilder", void 0, Gen_GenBuilder);
}

//# sourceMappingURL=coreOld.js.map
export function Gen_GenBuilder_$ctor() {
    return new Gen_GenBuilder();
}

//# sourceMappingURL=coreOld.js.map
export function Gen_GenBuilder__Return_1505(_, x) {
    return (s) => ((r) => [x, void 0]);
}

//# sourceMappingURL=coreOld.js.map
export function Gen_GenBuilder__ReturnFrom_Z781C29E4(_, x) {
    return curry(2, x);
}

//# sourceMappingURL=coreOld.js.map
export const Gen_loop = Gen_GenBuilder_$ctor();

//# sourceMappingURL=coreOld.js.map
export function Gen_preserve(factory, s, r) {
    const state = defaultArgWith(s, factory);
    return [state, state];
}

//# sourceMappingURL=coreOld.js.map
export function Gen_ofMutable(initialValue, s, r) {
    const refCell = defaultArgWith(s, () => (new FSharpRef(initialValue)));
    const setter = (value) => {
        refCell.contents = value;
    };
    return [[refCell.contents, setter], refCell];
}

//# sourceMappingURL=coreOld.js.map
export const Autos_loop = Gen_GenBuilder_$ctor();

//# sourceMappingURL=coreOld.js.map
