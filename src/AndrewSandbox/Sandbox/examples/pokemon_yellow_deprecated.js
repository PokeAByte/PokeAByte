// @ts-ignore
__variables;
// @ts-ignore
__state;
// @ts-ignore
__memory;
// @ts-ignore
const mapper = __mapper;
// @ts-ignore
__console;
function getValue(path) {
    // @ts-ignore
    const property = mapper.properties[path];
    if (!property) {
        throw new Error(`${path} is not defined in properties.`);
    }
    return property.value;
}
function setValue(path, value) {
    // @ts-ignore
    const property = mapper.properties[path];
    if (!property) {
        throw new Error(`${path} is not defined in properties.`);
    }
    property.value = value;
}

function preprocessor() {
    // FSM FOR GAMESTATE TRACKING
    // MAIN GAMESTATE: This tracks the three basic states the game can be in.
    // 1. "No Pokemon": cartridge reset; player has not received a Pokemon
    // 2. "Overworld": Pokemon in party, but not in battle
    // 3. "Battle": In battle
    if (getValue('player.team.0.level') === 0) {
        setValue('meta.state', 'No Pokemon');
    }
    else if (getValue("battle.type") === "None") {
        setValue('meta.state', 'Overworld');
    }
    else if (getValue("battle.turnInfo.battleStart") == 0) {
        setValue('meta.state', 'To Battle');
    }
    else if (getValue("battle.lowHealthAlarm") === "Disabled") {
        setValue('meta.state', 'From Battle');
    }
    else {
        setValue('meta.state', 'Battle');
    }
}

export { preprocessor };
