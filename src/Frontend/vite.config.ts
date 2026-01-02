import { defineConfig } from 'vite'
import preact from "@preact/preset-vite";
import { readFileSync } from 'fs';
import path from "path";

const webProject = readFileSync("../PokeAByte.Web/PokeAByte.Web.csproj", "utf-8");
const versionPrefix = webProject.match(/(?:<VersionPrefix>)((\d+\.)+\d)/)?.at(1);
const versionSuffix = webProject.match(/(?:<VersionSuffix>)(.+)(<\/VersionSuffix>)/)?.at(1);
const version = versionSuffix
	? versionPrefix+"-"+versionSuffix
	: versionPrefix;

export default defineConfig({
	plugins: [preact()],
	define: {
        '__POKEABYTE_VERSION__': JSON.stringify(version)
    },
	build: {
		target: ["safari17.2"]
	},
	resolve: {
		alias: {
			"@": path.resolve(__dirname, "./src"),
		},
	}
})
