export function containerprocessor(container, containerBytes) {
	if (!container.startsWith("script")) {
		return;
	}
	__console.log("wrote container bytes back to main memory");
	__console.trace("trace");
	__console.debug("debug");
	__console.info("info");
	__console.warn("warn");
	__console.error("error");
	__driver.WriteBytes(0, containerBytes);
}

export function preprocessor() {
	if (!__state.init) {
		__state.init = true;
		__memory.Fill("script", 0, [0, 0, 0,0]);
	}
}