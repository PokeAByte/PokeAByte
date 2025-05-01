import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { clipboardCopy } from "../utils/clipboardCopy";
import { CopyValueIcon } from "./CopyValueIcon";

export function PropertyInfoTable({ path }: { path: string }) {
	const type = useGamePropertyField(path, "type");
	const address = useGamePropertyField(path, "address");
	const length = useGamePropertyField(path, "length");
	const size = useGamePropertyField(path, "size");
	const reference = useGamePropertyField(path, "reference");

	return (
		<table className="stripes small-round surface small-space">
			<tbody>
				<tr>
					<th>Type</th>
					<td></td>
					<td>{type}</td>
				</tr>
				<tr>
					<th>Length</th>
					<td></td>
					<td>{length}</td>
				</tr>
				<tr>
					<th>Size</th>
					<td></td>
					<td>{size}</td>
				</tr>
				<tr>
					<th>Path</th>
					<td className="no-padding">
						<CopyValueIcon onClick={() => clipboardCopy(path)} />
					</td>
					<td>{path}</td>
				</tr>
				<tr>
					<th>Address</th>
					<td className="no-padding">
						<CopyValueIcon onClick={() => clipboardCopy(address?.toString())} />
					</td>
					<td>{address}</td>
				</tr>
				<tr>
					<th>Reference</th>
					<td className="no-padding">
						<CopyValueIcon onClick={() => clipboardCopy(reference)} />
					</td>
					<td>{reference}</td>
				</tr>
				<PropertyByteRow path={path} />
			</tbody>
		</table>
	);
}

export function PropertyByteRow({ path }: { path: string }) {
	const bytes = useGamePropertyField(path, "bytes");
	return (
		<tr>
			<th>Bytes</th>
			<td className="no-padding">
				<CopyValueIcon onClick={() => clipboardCopy(bytes?.join(""))} />
			</td>
			<td className="property-bytes">
				{bytes?.map((byte, i) => {
					return (
						<input
							key={i}
							type="text"
							size={1}
							maxLength={2}
							className="no-padding"
							value={byte.toString(16)}
							onChange={() => { }}
						/>
					);
				})}
			</td>
		</tr>
	);
}