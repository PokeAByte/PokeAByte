import { PropertyEdit } from "./PropertyEdit";
import { AttributesTable } from "./AttributesTable";
import { useStorageRecordState } from "../../../hooks/useStorageState";
import { clipboardCopy } from "../utils/clipboardCopy";
import { getPropertyFieldValue } from "./PropertyTextbox";
import { Store } from "@/utility/propertyStore";
import { CopyValueIcon } from "./CopyValueIcon";
import { VisibilityToggle } from "@/components/VisibilityToggle";
import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { hiddenOverrideSignal, hiddenProperties } from "@/Contexts/hiddenPropertySignal";
import { useCallback } from "preact/hooks";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";
import { Show } from "@preact/signals/utils";

export function PropertyValue({ path, mapperId }: { mapperId: string, path: string }) {
	const [isTableOpen, setTableOpen] = useStorageRecordState(mapperId+"-attributes", path, false);
	const override = hiddenOverrideSignal.value;
	const bits = useGamePropertyField(path, "bits");
	const address = useGamePropertyField(path, "address");
	const toggleTable =  useCallback(
		() => setTableOpen(!isTableOpen),
		[setTableOpen, isTableOpen]
	);
	const handleCopyClick = useCallback(() => {
		const property = Store.getProperty(path);
		if (property) {
			clipboardCopy(getPropertyFieldValue(property?.value, property.type));
		}
	}, [path]);
	if (!override && hiddenProperties.value.includes(path)) {
		return null;
	}

	let addressString = address ? `0x${address.toString(16).toUpperCase()}` : "";
	if (bits) {
		addressString += bits.includes("-")
			? ` (bits: ${bits})`
			: ` (bit: ${bits})`
	}
	return (
		<>
			<tr class="property striped">
				<Show 
					when={advancedModeSignal} 
					fallback={
						<th >
							<label htmlFor={"edit-" + path} >
								{path.split(".").pop()}:
							</label>
						</th>
					}
				>
					<th onClick={() => toggleTable()} class="interactive">
						<label htmlFor={"edit-" + path} >
							{path.split(".").pop()}:
						</label>
					</th>
				</Show>
				<td>
					<CopyValueIcon onClick={handleCopyClick} />
					<PropertyEdit path={path} />
					<Show when={advancedModeSignal}>
						<span class="color-darker center-self">
							{addressString}
						</span>
						<VisibilityToggle path={path} />
					</Show>
				</td>
			</tr>
			
			<Show when={advancedModeSignal} fallback={<tr class="hidden" />}>
				{isTableOpen 
					? <tr>
						<td colSpan={2}>
							<AttributesTable path={path} />
						</td>
					</tr>
					: <tr class="hidden" />
				}
			</Show>
		</>
	);
}

