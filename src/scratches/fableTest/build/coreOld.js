import { FSharpRef, Record } from "./.fable/fable-library.3.2.9/Types.js";
import { record_type, obj_type, class_type } from "./.fable/fable-library.3.2.9/Reflection.js";
import { defaultArgWith, some } from "./.fable/fable-library.3.2.9/Option.js";
import { curry, partialApply } from "./.fable/fable-library.3.2.9/Util.js";

export class Gen_BoxedState extends Record {
    constructor(stateType, state) {
        super();
        this.stateType = stateType;
        this.state = state;
    }
}

export function Gen_BoxedState$reflection() {
    return record_type("LocSta.Gen.BoxedState", [], Gen_BoxedState, () => [["stateType", class_type("System.Type")], ["state", obj_type]]);
}

export function Gen_BoxedState_Create_1505(state) {
    let copyOfStruct;
    return new Gen_BoxedState((copyOfStruct = state, null), state);
}

export class Gen_CombinedBoxedState extends Record {
    constructor(mState, fState) {
        super();
        this.mState = mState;
        this.fState = fState;
    }
}

export function Gen_CombinedBoxedState$reflection() {
    return record_type("LocSta.Gen.CombinedBoxedState", [], Gen_CombinedBoxedState, () => [["mState", Gen_BoxedState$reflection()], ["fState", Gen_BoxedState$reflection()]]);
}

function Gen_unboxState(state) {
    if (state != null) {
        const x = state;
        return some(x.state);
    }
    else {
        return void 0;
    }
}

export class Gen_GenBuilder {
    constructor() {
    }
}

export function Gen_GenBuilder$reflection() {
    return class_type("LocSta.Gen.GenBuilder", void 0, Gen_GenBuilder);
}

export function Gen_GenBuilder_$ctor() {
    return new Gen_GenBuilder();
}

export function Gen_GenBuilder__Bind_456328F3(this$, m, f) {
    return (mfState) => ((r) => {
        const m_1 = m;
        const f_1 = f;
        const mfState_1 = mfState;
        const r_1 = r;
        let patternInput;
        if (mfState_1 != null) {
            const mfState_2 = mfState_1;
            patternInput = [mfState_2.mState, mfState_2.fState];
        }
        else {
            patternInput = [void 0, void 0];
        }
        const mState = patternInput[0];
        const fState = patternInput[1];
        const patternInput_1 = m_1(Gen_unboxState(mState), r_1);
        const mState$0027 = patternInput_1[1];
        const mOut = patternInput_1[0];
        const fgen = partialApply(2, f_1, [mOut]);
        const patternInput_2 = fgen(Gen_unboxState(fState))(r_1);
        const fState$0027 = patternInput_2[1];
        const fOut = patternInput_2[0];
        const resultingState = new Gen_CombinedBoxedState(Gen_BoxedState_Create_1505(mState$0027), Gen_BoxedState_Create_1505(fState$0027));
        return [fOut, resultingState];
    });
}

export function Gen_GenBuilder__Return_1505(this$, x) {
    return (s) => ((r) => [x, void 0]);
}

export function Gen_GenBuilder__ReturnFrom_Z781C29E4(this$, x) {
    return curry(2, x);
}

export const Gen_loop = Gen_GenBuilder_$ctor();

export function Gen_preserve(factory, s, r) {
    const state = defaultArgWith(s, factory);
    return [state, state];
}

export function Gen_ofMutable(initialValue, s, r) {
    const refCell = defaultArgWith(s, () => (new FSharpRef(initialValue)));
    const setter = (value) => {
        refCell.contents = value;
    };
    return [[refCell.contents, setter], refCell];
}

export const Autos_loop = Gen_GenBuilder_$ctor();

//# sourceMappingURL=coreOld.js.map
