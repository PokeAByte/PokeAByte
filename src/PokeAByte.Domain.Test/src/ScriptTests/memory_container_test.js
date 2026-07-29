

export function read_raw_bytes(property) {
	const memory = Array.from(__memory.defaultNamespace.get_raw_bytes(0, 4));
	__console.log("get_raw_bytes: " + JSON.stringify(memory));
	return false;
}

export function read_all_bytes(property) {
	const memory = Array.from(__memory.defaultNamespace.GetAllBytes());
	__console.log("GetAllBytes: " + JSON.stringify(memory));
	return false;
}