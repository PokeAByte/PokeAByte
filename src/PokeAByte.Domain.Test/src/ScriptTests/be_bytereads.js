export function postprocessor() {
	// Test the IByteArray interface through get_bytes():
	const int16 = __memory.defaultNamespace.get_bytes(0, 2).get_uint16_be(0);
	const int32 = __memory.defaultNamespace.get_bytes(0, 6).get_uint32_be(2);
	const int64 = __memory.defaultNamespace.get_bytes(0, 14).get_uint64_be(6);
	__mapper.set_property_value("test.int16", int16);
	__mapper.set_property_value("test.int32", int32);
	__mapper.set_property_value("test.int64", int64);
	
	// Test the namespace shorthand functions:
	const directInt16 = __memory.defaultNamespace.get_uint16_be(0);
	const directInt32 = __memory.defaultNamespace.get_uint32_be(2);
	const directInt64 = __memory.defaultNamespace.get_uint64_be(6);
	__mapper.set_property_value("test.directInt16", directInt16);
	__mapper.set_property_value("test.directInt32", directInt32);
	__mapper.set_property_value("test.directInt64", directInt64);
}