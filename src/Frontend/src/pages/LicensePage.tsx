// @ts-expect-error Bogus TS error about the raw text import.
import license from "../../LICENSE.txt?raw";

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
			<h3>Third party licenses (Frontend)</h3>
			<table>
				<thead>
					<tr>
						<th class="min">Package</th>
						<th class="min">Copyright</th>
						<th class="min">License</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td class="min">classnames</td>
						<td class="min">Jed Watson</td>
						<td class="min">MIT</td>
					</tr>
					<tr>
						<td class="min">deep-equal</td>
						<td class="min">James Hallida</td>
						<td class="min">MIT</td>
					</tr>
					<tr>
						<td class="min">pokeaclient</td>
						<td class="min">StringEpsilon</td>
						<td class="min">Apache-2.0</td>
					</tr>
					<tr>
						<td class="min">preact</td>
						<td class="min">Jason Miller</td>
						<td class="min">MIT</td>
					</tr>
					<tr>
						<td class="min">wouter</td>
						<td class="min">Alexey Taktarov</td>
						<td class="min">Unlicense license</td>
					</tr>
					<tr>
						<td class="min">Material Icons</td>
						<td class="min">Google</td>
						<td class="min">Apache-2.0 license </td>
					</tr>
					<tr>
						<td class="min">Roboto (font)</td>
						<td class="min">The Roboto Project Authors</td>
						<td class="min"> SIL Open Font License, Version 1.1</td>
					</tr>
					<tr>
						<td class="min">Roboto Mono (font)</td>
						<td class="min">Christian Robertson</td>
						<td class="min">Apache-2.0 license</td>
					</tr>
				</tbody>
			</table>
			<h3>Third party licenses (Backend)</h3>
			<table>
				<thead>
					<tr>
						<th class="min">Package</th>
						<th class="min">Copyright</th>
						<th class="min">License</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td class="min">CoreCLR-NCalc</td>
						<td class="min">Sebastien Ros, Sebastian Klose</td>
						<td class="min">MIT</td>
					</tr>
					<tr>
						<td class="min">Jint</td>
						<td class="min">Sebastien Ros</td>
						<td class="min">BSD-2-Clause</td>
					</tr>
					<tr>
						<td class="min">Serilog</td>
						<td class="min">Serilog Contributors</td>
						<td class="min">Apache-2.0 license</td>
					</tr>
				</tbody>
			</table>
		</article>
	)
};
