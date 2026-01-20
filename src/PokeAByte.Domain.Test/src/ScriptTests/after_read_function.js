export function increment_property(property) {
	return property.fullValue 
		? property.fullValue + 1
		: null;
}