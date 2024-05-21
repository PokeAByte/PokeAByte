// @ts-ignore
const variables = __variables;
// @ts-ignore
__state;
// @ts-ignore
const memory = __memory;
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
    //decrypt the boxmon substructure
    let startingAddress = 0x20244EC;
    for(let slotIndex = 0; slotIndex < 6; slotIndex++) {
        let offset = slotIndex * 100;
        //Get the entire pokemon data (100 bytes long)
        let pokemonData = memory.defaultNamespace.get_bytes(startingAddress + offset, 100);

        //Get the personality value
        let pid = pokemonData.get_uint32_le();
        //Get the trainer id
        let otId =  pokemonData.get_uint32_le(4);
        let checksum = pokemonData.get_uint16_le(28);
        log(checksum);
        sleep(10000);
    }
}

//Todo: build this into the property readloop method
function readPlayerDataFunction(obj) {
    //Due to how the player data works for emerald, a pointer is stored in 0x03005D90 (IWRAM) which tells us
    //where the player data is located in EWRAM. I could be wrong, however, I believe the data in the EWRAM is
    //swapped out during transitions and then reinserted at different locations which is why we need to first
    //grab the pointer
    const addr = obj.pointerAddress;
    const offset = obj.pointerAddressOffset;
    if(addr != null) {
        const memoryAddress = memory.defaultNamespace.get_uint32_le(addr)
        if(memoryAddress === 0x00)
            return;
        obj.address = memoryAddress+offset;
    }
}

export { readPlayerDataFunction };