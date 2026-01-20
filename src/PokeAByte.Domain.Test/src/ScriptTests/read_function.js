export function map_bytes(property) {
	if (__memory.defaultNamespace.get_byte(0) == 0) {
		property.value = "False";
	} else {
		property.value = "True";
	}
	return false;
}