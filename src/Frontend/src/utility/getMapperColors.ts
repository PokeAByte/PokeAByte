type MapperColors = { border: string, bg: string, text: string }

const COLOR_MAP: Record<string, MapperColors> = {
	"pokemon yellow": { border: "#FFD733", bg: "#FFD733", text: "black" },
	"pokemon red blue": { border: "#DA3914", bg: "#DA3914", text: "white" },

	"pokemon gold": { border: "#E7C46E", bg: "#DAA520", text: "black" },
	"pokemon crystal": { border: "#4FD9FF", bg: "#8CE6FF", text: "black" },

	"pokemon ruby": { border: "#CD2236", bg: "#DF6F7C", text: "white" },
	"pokemon emerald": { border: "#009652", bg: "#59BB8F", text: "white" },
	"pokemon firered leafgreen": { border: "#F15C01", bg: "#F15C01", text: "white" },

	"pokemon diamond": { border: "#5E7C9A", bg: "#90BEED", text: "black" },
	"pokemon platinum": { border: "#C1C1B5", bg: "#A0A08D", text: "black" },
	"pokemon heartgold": { border: "#F0CF5B", bg: "#E8B502", text: "black" },

	"pokemon white 2": { border: "#F2D9D8", bg: "#F2D9D8", text: "black" },
	"pokemon black 2": { border: "#1F2835", bg: "#1F2835", text: "white" },
	"pokemon white": { border: "#EBEBEB", bg: "#EBEBEB", text: "black" },
	"pokemon black": { border: "#444444", bg: "#444444", text: "white" },
}

export function getMapperColors(mapper: string) {
	mapper = mapper.toLowerCase();
	const colorKey = Object.getOwnPropertyNames(COLOR_MAP).find(x => mapper.includes(x));
	if (colorKey) {
		return COLOR_MAP[colorKey] ?? null;
	}
	return null;
}