import React from "react";
import { getPropertyFieldValue, PropertyInputField } from "./PropertyInputField";
import { CopyValueIcon } from "./CopyValueIcon";
import { Store } from "../../../utility/propertyStore";
import { PropertyInfoTable } from "./PropertyInfoTable";
import { clipboardCopy } from "../utils/clipboardCopy";

export function PropertyInput({ path }: { path: string }) {
	const [tableOpen, setTableOpen] = React.useState(false);

	const handleCopyClick = () => {
		const currentPropValue = Store.getProperty(path);
		if (currentPropValue) {
			clipboardCopy(getPropertyFieldValue(currentPropValue.value, currentPropValue.type));
		}
	};

	return (
		<li className="property">
			<label htmlFor={"edit-" + path} onClick={() => setTableOpen(!tableOpen)}>
				{path.split(".").pop()}:
			</label>
			<div>
				<div>
					<CopyValueIcon onClick={handleCopyClick} />
					<PropertyInputField path={path} />
				</div>
				{tableOpen &&
					<PropertyInfoTable path={path} />
				}
			</div>
		</li>
	);
}

