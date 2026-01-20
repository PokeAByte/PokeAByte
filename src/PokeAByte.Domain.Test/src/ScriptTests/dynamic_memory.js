export function preprocessor() {
	__memory.fill(
		"script_memory", 
		0x00, 
		[
			0, 16,
			0, 0, 0, 32,
		]
	);
}
