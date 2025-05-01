import React, { SyntheticEvent } from "react";
import { Store } from "../../../utility/propertyStore";
import { PropertyInput } from "./PropertyInput";
import { unique } from "../utils/unique";

function getPropertyOpenState(mapperId: string | undefined, path: string): boolean {
	if (!mapperId) {
		return false;
	}
	const mapperConfigJson = window.localStorage.getItem(mapperId);
	if (mapperConfigJson == null) {
		return false;
	}
	const mapperConfig = JSON.parse(mapperConfigJson);
	return !!(mapperConfig[path]);
}

function savePropertyOpenState(mapperId: string | undefined, path: string, isOpen: boolean) {
	if (!mapperId) {
		return;
	}
	const mapperConfigJson = window.localStorage.getItem(mapperId);
	let mapperConfig: Record<string, boolean> = {};
	if (mapperConfigJson != null) {
		mapperConfig = JSON.parse(mapperConfigJson);
	}
	mapperConfig[path] = isOpen;
	window.localStorage.setItem(mapperId, JSON.stringify(mapperConfig));
	return !!(mapperConfig[path]);
}

export function PropertyTree({ path, level = 1 }: { path: string, level?: number }) {
	const properties = Store.getAllProperties();
	const mapperId = Store.getMapper()?.id;

	const [isOpen, setIsOpen] = React.useState(getPropertyOpenState(mapperId, path));
	const onToggleOpen = (event: SyntheticEvent<HTMLDetailsElement, Event>) => {
		savePropertyOpenState(mapperId, path, event.currentTarget.open);
		setIsOpen(event.currentTarget.open);
	}
	if (properties[path]) {
		return <PropertyInput path={path} />
	}
	const immediateChildren = Object.keys(properties)
		.filter(x => x.startsWith(path + "."))
		.map(x => ({name: x.split(".")[level], level: x.split('.').length}))
		.toSorted((a, b) => {
			if (a.level > b.level) {
				return 1;
			}
			if (a.level < b.level) {
				return -1;
			}
			return 0;
		})
		.map(x => x.name)
		.filter(unique);

	return (
		<li>
			<details open={isOpen} onToggle={onToggleOpen}>
				<summary className="folder" >
					<strong>{path}</strong> {immediateChildren.length} Entries
				</summary>
				<ul>
					{isOpen &&
						immediateChildren.map(x => {
							const childPath = path + "." + x;
							return <PropertyTree key={childPath} path={childPath} level={level + 1} />;
						})
					}
				</ul>
			</details>
		</li>
	)
}