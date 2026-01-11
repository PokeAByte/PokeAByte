// @ts-expect-error Bogus TS error about the raw text import.
import license from "@/../LICENSE.txt?raw";
import { AttributionTable } from "./AttributionTable";

/** Third party library attribution data. */
export type Attribution = { 
	name: string, 
	authors: string, 
	license: string 
};

/** Attributions for the .NET part / server side. */
const backendAttributions: Attribution[] = [
	{ name: "PKHeX", authors: "PKHeX contributors", license: "GPLv3" },
	{ name: "CoreCLR-NCalc", authors: "Sebastien Ros, Sebastian Klose", license: "MIT" },
	{ name: "Jint", authors: "Sebastien Ros", license: "BSD-2-Clause" },
	{ name: ".NET runtime", authors: ".NET Foundation and Contributors", license: "MIT" },
	{ name: "ASP.NET Core", authors: ".NET Foundation and Contributors", license: "MIT" },
];

/** Attributions for the JavaScript part / browser side. */
const frontendAttributions: Attribution[] = [
	{ name: "pokeaclient", authors: "StringEpsilon", license: "Apache-2.0" },
	{ name: "preact", authors: "Jason Miller", license: "MIT" },
	{ name: "@preact/signals", authors: "Preact Team", license: "MIT" },
	{ name: "Material Icons", authors: "Google", license: "Apache-2.0 license " },
	{ name: "Roboto (font)", authors: "The Roboto Project Authors", license: "SIL Open Font License, Version 1.1" },
	{ name: "Roboto Mono (font)", authors: "Christian Robertson", license: "Apache-2.0 license" }
];

/** Renders the page with license information, including attribution of third party libraries. */
export function LicensePage() {
	return (
		<article class="license">
			<h2>License</h2>
			<p>
				Poke-A-Byte is available under the GNU Affero General Public License. <br />
				The source code is hosted at <a href="https://github.com/PokeAByte/PokeAByte">https://github.com/PokeAByte/PokeAByte</a>
			</p>
			<div>
				<details >
					<summary> AGPL license text</summary>
					<pre>
						{license}
					</pre>
				</details>
			</div>
			<hr />
			<h2>Third party licenses</h2>
			<table class="striped">
				<thead>
					<tr>
						<th>Package</th>
						<th>Copyright</th>
						<th>License</th>
					</tr>
				</thead>
				<tbody>
					<AttributionTable title="Frontend:" items={frontendAttributions} />
					<AttributionTable title="Backend:" items={backendAttributions} />
				</tbody>
			</table>
		</article>
	)
};
