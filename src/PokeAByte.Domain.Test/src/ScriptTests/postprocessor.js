
export function preprocessor() {
	const property = __mapper.get_property_value("test.pre");
	__mapper.set_property_value("test.pre", property + 1);
}

export function postprocessor() {
	const property = __mapper.get_property_value("test.post");
	__mapper.set_property_value("test.post", property + 1);
}
