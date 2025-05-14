
import { Store } from "../../../utility/propertyStore";
import { PropertyValue } from "./PropertyValue";
import { unique } from "../utils/unique";
import { useStorageRecordState } from "../../../hooks/useStorageState";
import { useContext, useState } from "preact/hooks";
import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { HidePropertyContext, IfNotHidden, ToggleHidden } from "../../../Contexts/HidePropertyContext";
import { Advanced } from "../../../Contexts/Advanced";

function isNumeric(x: any) {
	return parseInt(x).toString() == x;
}

export function PropertyTree({ path, level = 1 }: { path: string, level?: number }) {
	const properties = Store.getAllProperties();
	const hideContext = useContext(HidePropertyContext);
	const hiddenItemCount = Object.keys(properties)
		.filter(x => x.startsWith(path + "."))
		.filter(x => !hideContext.override && hideContext.hiddenProperties.includes(x))
		.length
	const immediateChildren = Object.keys(properties)
		.filter(x => x.startsWith(path + "."))
		.filter(x => hideContext.override || !hideContext.hiddenProperties.includes(x))
		.map(x => ({ name: x.split(".")[level], level: x.split('.').length }))
		.toSorted((a, b) => {
			if (a.level > b.level) return 1;
			if (a.level < b.level) return -1;
			return 0;
		})
		.map(x => x.name)
		.filter(unique);
	const mapperId = Store.getMapper()!.id;
	const [isOpen, setIsOpen] = useStorageRecordState(mapperId, path, false);
	const [[name, secondaryNameProperty] ] = useState(() => {
		const pathSements = path.split(".");
		const name = pathSements[pathSements.length - 1];
		let secondaryNameProperty = "";
		if (isNumeric(name)) {
			if (properties[path + ".species"]) {
				secondaryNameProperty = path + ".species";
			} else {
				secondaryNameProperty = path +  "." + immediateChildren[0];
			}
		}
		return [name, secondaryNameProperty];
	});
	const secondaryName = useGamePropertyField(secondaryNameProperty, "value");
	const onToggleOpen = () => setIsOpen(!isOpen);

	if (properties[path]) {
		return (
			<IfNotHidden path={path}>
				<PropertyValue mapperId= {mapperId} path={path} />
			</IfNotHidden>
		);
	}

	return (
		<>
			<tr class="leaf interactive" onClick={onToggleOpen}>
				<th >
					<i className="material-icons"> {isOpen ? "folder" : "folder_open"} </i>
					<span class="margin-left">
						{name}
						{secondaryName &&
							<span> - {secondaryName?.toString()} </span>
						}
					</span>
				</th>
				<td>
					<span class="margin-left color-darker">
						{immediateChildren.length} Entries 
						{hiddenItemCount > 0 && ` (+${hiddenItemCount} hidden)`}
					</span>
					<Advanced>
						<ToggleHidden path={path} />
					</Advanced>
				</td>
			</tr>
			<tr class={isOpen ? "" : "hidden"}>
				<td colSpan={2}>
					{isOpen &&
						<table class="property-table">
							<tbody>
								{immediateChildren.map(x => {
									const childPath = path + "." + x;
									return <PropertyTree key={childPath} path={childPath} level={level + 1} />;
								})}
							</tbody>
						</table>
					}
				</td>
			</tr>
		</>
	)
}