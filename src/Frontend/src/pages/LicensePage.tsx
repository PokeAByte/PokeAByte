// @ts-expect-error Bogus TS error about the raw text import.
import license from "../../LICENSE.txt?raw";

export function LicensePage() {
	return (
		<article className="layout-box license">
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
						<th className="min">Package</th>
						<th className="min">Copyright</th>
						<th className="min">License</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td className="min">classnames</td>
						<td className="min">Jed Watson</td>
						<td className="min">MIT</td>
					</tr>
					<tr>
						<td className="min">deep-equal</td>
						<td className="min">James Hallida</td>
						<td className="min">MIT</td>
					</tr>
					<tr>
						<td className="min">pokeaclient</td>
						<td className="min">StringEpsilon</td>
						<td className="min">Apache-2.0</td>
					</tr>
					<tr>
						<td className="min">preact</td>
						<td className="min">Jason Miller</td>
						<td className="min">MIT</td>
					</tr>
					<tr>
						<td className="min">wouter</td>
						<td className="min">Alexey Taktarov</td>
						<td className="min">Unlicense license</td>
					</tr>
					<tr>
						<td className="min">Material Icons</td>
						<td className="min">Google</td>
						<td className="min">Apache-2.0 license </td>
					</tr>
					<tr>
						<td className="min">Roboto (font)</td>
						<td className="min">The Roboto Project Authors</td>
						<td className="min"> SIL Open Font License, Version 1.1</td>
					</tr>
					<tr>
						<td className="min">Roboto Mono (font)</td>
						<td className="min">Christian Robertson</td>
						<td className="min">Apache-2.0 license</td>
					</tr>
				</tbody>
			</table>
			<h3>Third party licenses (Backend)</h3>
			<table>
				<thead>
					<tr>
						<th className="min">Package</th>
						<th className="min">Copyright</th>
						<th className="min">License</th>
					</tr>
				</thead>
				<tbody>
					<tr>
						<td className="min">CoreCLR-NCalc</td>
						<td className="min">Sebastien Ros, Sebastian Klose</td>
						<td className="min">MIT</td>
					</tr>
					<tr>
						<td className="min">Jint</td>
						<td className="min">Sebastien Ros</td>
						<td className="min">BSD-2-Clause</td>
					</tr>
					<tr>
						<td className="min">Serilog</td>
						<td className="min">Serilog Contributors</td>
						<td className="min">Apache-2.0 license</td>
					</tr>
				</tbody>
			</table>
		</article>
	)
};
