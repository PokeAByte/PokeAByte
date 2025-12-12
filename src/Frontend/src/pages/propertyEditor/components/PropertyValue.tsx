import { PropertyEdit } from "./PropertyEdit";
import { AttributesTable } from "./AttributesTable";
import { NotAdvanced } from "../../../components/NotAdvanced";
import { Advanced } from "../../../components/Advanced";
import { useStorageRecordState } from "../../../hooks/useStorageState";

export function PropertyValue({ path, mapperId }: { mapperId: string, path: string }) {
	const [isTableOpen, setTableOpen] = useStorageRecordState(mapperId+"-attributes", path, false);
	const toggleTable = () => setTableOpen(!isTableOpen);
	return (
		<>
			<tr className="property striped">
				<Advanced>
					<th onClick={() => toggleTable()} class={"interactive"}>
						<label htmlFor={"edit-" + path} >
							{path.split(".").pop()}:
						</label>
					</th>
				</Advanced>
				<NotAdvanced>
					<th >
						<label htmlFor={"edit-" + path} >
							{path.split(".").pop()}:
						</label>
					</th>
				</NotAdvanced>
				<td>
					<PropertyEdit path={path} />
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
			<NotAdvanced>
				<tr class="hidden" />
			</NotAdvanced>
		</>
	);
}

