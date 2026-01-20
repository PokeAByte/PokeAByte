export function containerprocessor(container, containerBytes) {
	if (!container.startsWith("script")) {
		return;
	}
	__console.log("wrote container bytes back to main memory");
	__driver.WriteBytes(0, containerBytes);
}

export function preprocessor() {
	if (!__state.init) {
		__state.init = true;
		__memory.Fill("script", 0, [0, 0, 0,0]);
	}
}