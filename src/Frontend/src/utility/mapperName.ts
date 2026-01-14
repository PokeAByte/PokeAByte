export function getMapperCategory(path: string) {
	const segments = path.split("/");
	return segments.length == 2
		? segments[0].toLocaleUpperCase()
		: segments[1].toLocaleLowerCase();
}

export function beautifyMapperName(mapper: {path: string, display_name: string, type?: number}) {
	const segments = mapper.path.split("/");
	let name = mapper.display_name.replace(".xml", "").replaceAll("_", " ");
	if (mapper.type == 1) {
		name += " (local)"
	}
	if (segments.length > 1) {
		return `(${getMapperCategory(mapper.path)}) ${name}`;
	}
	return name;
}
