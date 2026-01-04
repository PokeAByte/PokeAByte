
import { Store } from "../../../utility/propertyStore";
import { PropertyValue } from "./PropertyValue";
import { unique } from "../../../utility/unique";
import { useStorageRecordState } from "../../../hooks/useStorageState";
import { useContext, useState } from "preact/hooks";
import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { HidePropertyContext } from "../../../Contexts/HidePropertyContext";
import { VisibilityToggle } from "../../../components/VisibilityToggle";
import { IfNotHidden } from "../../../components/IfNotHidden";
import { Advanced } from "../../../components/Advanced";
import { GameProperty } from "pokeaclient";

function isNumeric(x: any) {
	return parseInt(x).toString() == x;
}

function matchProperty(property: GameProperty<any>, query: string) {
	return property.path.toLocaleLowerCase().includes(query)
		|| property.address?.toString(16) === query
}

export function PropertyTree({ path, level = 1, search = "" }: { path: string, level?: number, search: string }) {
	const properties = Store.getAllProperties();
	const hideContext = useContext(HidePropertyContext);
	const hiddenItemCount = Object.keys(properties)
		.filter(x => x.startsWith(path + "."))
		.filter(x => !hideContext.override && hideContext.hiddenProperties.includes(x))
		.length

	let immediateChildren: string[];
	if (search) {
		let query = search.toLowerCase();
		if (query.startsWith("0x")) {
			query = query.substring(2);
		}
		immediateChildren = Object.keys(properties)
			.filter(x => x.startsWith(path + ".") && matchProperty(properties[x], query))
			.map(x => { 
				const levels = x.split(".");
				return { name: levels[level], level: levels.length, path: x }
			})
			.toSorted((a, b) => {
				if (a.level > b.level) return 1;
				if (a.level < b.level) return -1;
				return 0;
			})
			.map(x => x.name)
			.filter(unique);
	} else {
		immediateChildren = Object.keys(properties)
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
	}
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
	if (search && immediateChildren.length == 0) {
		return null;
	}
	const openOverride = search && (immediateChildren.length <= 6 || level === 1);
	return (
		<>
			<PropertyTreeHeader 
				isOpen={isOpen}
				onToggleOpen={onToggleOpen}
				path={path}
				name={name}
				secondaryName={secondaryName}
				entryCount={immediateChildren.length}
				hiddenEntryCount={hiddenItemCount}
			/>
			<tr class={(isOpen || openOverride) ? "" : "hidden"}>
				<td colSpan={2}>
					{(isOpen || openOverride) &&
						<table class="property-table">
							<tbody>
								{immediateChildren.map(x => {
									const childPath = path + "." + x;
									return <PropertyTree key={childPath} path={childPath} level={level + 1} search={search} />;
								})}
							</tbody>
						</table>
					}
				</td>
			</tr>
		</>
	)
}

type PropertyTreeHeaderProps = {
	onToggleOpen: () => void,
	isOpen: boolean,
	name: string,
	secondaryName?: string,
	entryCount: number,
	hiddenEntryCount: number,
	path: string,
}

function PropertyTreeHeader(props: PropertyTreeHeaderProps) {
	return (
		<tr class="leaf interactive" onClick={props.onToggleOpen}>
			<th >
				<i class="material-icons"> {props.isOpen ? "folder" : "folder_open"} </i>
				<span class="margin-left">
					{props.name}
					{props.secondaryName &&
						<span> - {props.secondaryName?.toString()} </span>
					}
				</span>
			</th>
			<td>
				<span class="margin-left color-darker">
					{props.entryCount} Entries 
					{props.hiddenEntryCount > 0 && ` (+${props.hiddenEntryCount} hidden)`}
				</span>
				<Advanced>
					<VisibilityToggle path={props.path} />
				</Advanced>
			</td>
		</tr>
	);
}