import { PropertyEdit } from "./PropertyEdit";
import { AttributesTable } from "./AttributesTable";
import { NotAdvanced } from "../../../Contexts/NotAdvanced";
import { Advanced } from "../../../Contexts/Advanced";
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
					{/* <i class="material-icons hide-icon margin-right"> visibility_off </i> */}
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

