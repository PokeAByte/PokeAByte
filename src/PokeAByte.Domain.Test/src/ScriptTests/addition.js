
export function preprocessor() {
	const a = __mapper.get_property_value("test.game-memory-1");
	const b = __mapper.get_property_value("test.game-memory-2");
	__mapper.set_property_value("test.script-set", a + b);
}