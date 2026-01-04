import { PropertyEdit } from "./PropertyEdit";
import { AttributesTable } from "./AttributesTable";
import { Advanced } from "../../../components/Advanced";
import { useStorageRecordState } from "../../../hooks/useStorageState";
import { clipboardCopy } from "../utils/clipboardCopy";
import { getPropertyFieldValue } from "./PropertyTextbox";
import { Store } from "@/utility/propertyStore";
import { CopyValueIcon } from "./CopyValueIcon";
import { VisibilityToggle } from "@/components/VisibilityToggle";
import { useGamePropertyField } from "../hooks/useGamePropertyField";

export function PropertyValue({ path, mapperId }: { mapperId: string, path: string }) {
	const [isTableOpen, setTableOpen] = useStorageRecordState(mapperId+"-attributes", path, false);
	const bits = useGamePropertyField(path, "bits");
	const address = useGamePropertyField(path, "address");
	const toggleTable = () => setTableOpen(!isTableOpen);
	const handleCopyClick = () => {
		const property = Store.getProperty(path);
		if (property) {
			clipboardCopy(getPropertyFieldValue(property?.value, property.type));
		}
	};
	let addressString = address ? `0x${address.toString(16).toUpperCase()}` : "";
	if (bits) {
		addressString += bits.includes("-")
			? ` (bits: ${bits})`
			: ` (bit: ${bits})`
	}
	return (
		<>
			<tr class="property striped">
				<Advanced>
					<th onClick={() => toggleTable()} class="interactive">
						<label htmlFor={"edit-" + path} >
							{path.split(".").pop()}:
						</label>
					</th>
				</Advanced>
				<Advanced when={false}>
					<th >
						<label htmlFor={"edit-" + path} >
							{path.split(".").pop()}:
						</label>
					</th>
				</Advanced>
				<td>
					<CopyValueIcon onClick={handleCopyClick} />
					<PropertyEdit path={path} />
					<Advanced>
						<span class="color-darker center-self">
							{addressString}
						</span>
					</Advanced>
					<Advanced>
						<VisibilityToggle path={path} />
					</Advanced>
				</td>
			</tr>
			<Advanced>
				{isTableOpen 
					? <tr>
						<td colSpan={2}>
							<AttributesTable path={path} />
						</td>
					</tr>
					: <tr class="hidden" />
				}
			</Advanced>
			<Advanced when={false}>
				<tr class="hidden" />
			</Advanced>
		</>
	);
}

