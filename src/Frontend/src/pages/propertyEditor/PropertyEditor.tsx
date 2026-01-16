import { Store } from "@/utility/propertyStore";
import { PropertyTree } from "./components/PropertyTree";
import { useEffect, useState } from "preact/hooks";
import { navigateTo } from "@/components/Route";
import { CollapseAllButton, ForceVisibilityToggle } from "@/components/ForceVisibilityToggle";
import { IfNotHidden } from "@/components/IfNotHidden";
import { IconButton } from "@/components/IconButton";
import { isConnectedSignal, mapperSignal } from "@/Contexts/mapperSignal";
import { signal } from "@preact/signals";
import { Show } from "@preact/signals/utils";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";

function isNumeric(x: any) {
	return parseInt(x).toString() == x;
}


export type PropertyTreeNode = {
	key: string,
	path: string,
	children?: PropertyTreeNode[],
	secondaryNamePath?: string,
	allChildren?: string[],
};

export function createPropertyTree(paths: string[]) {
	const tree: PropertyTreeNode[] = [];
	addToPropertyTree(tree, paths);
	return tree;
}

function addToPropertyTree(tree: PropertyTreeNode[], paths: string[], depth: number = 0, parent: PropertyTreeNode | null = null) {
	paths.forEach(path => {
		const fragments = path.split('.');
		const key = fragments[depth];
		let node = tree.find(x => x.key === key);
		if (!node) {
			node = {
				key: key,
				path: fragments.slice(0, depth + 1).join(".")
			}
			tree.push(node);
		}
		if (fragments.length > depth + 1) {
			if (!node.children) {
				node.children = [];
				node.allChildren = [];
			}
			addToPropertyTree(node.children, [path], depth + 1, node);
			node.children.sort((a, b) => {
				if ((a.children?.length ?? 0) === 0 && (b.children?.length ?? 0) !== 0) return -1;
				if ((b.children?.length ?? 0) === 0 && (a.children?.length ?? 0) !== 0) return 1;
				return 0;
			});
		}
		if (isNumeric(key)) {
			if (node.allChildren?.some((x) => x === node.path + ".species")) {
				node.secondaryNamePath = node.path + ".species";
			} else {
				node.secondaryNamePath = node.children?.at(0)?.path;
			}
		}
		parent?.allChildren?.push(path)
	});
}
export const propertySearchSignal = signal("");

export function PropertyEditor() {
	const [propertyTree, setPropertyTree] = useState(() => createPropertyTree(Object.keys(Store.getAllProperties())));
	const mapper = mapperSignal.value;
	const isConnected = isConnectedSignal.value;

	useEffect(() => {
		if (!mapper || !isConnected) {
			navigateTo("/mappers");
		}
		setPropertyTree(createPropertyTree(Object.keys(Store.getAllProperties())));
	}, [mapper, isConnected])

	if (!isConnected || !mapper) {
		return (
			<div id="property-editor">
				<h1>
					No connection to PokeAByte
				</h1>
			</div>
		);
	}

	return (
		<div class="layout-box margin-top" >
			<div class="title">
				<div>
					<strong>{mapper.gameName}</strong>
				</div>
				<div>
					<Show when={advancedModeSignal}>
						<ForceVisibilityToggle />
					</Show>
					<CollapseAllButton />
				</div>
			</div>
			<PropertySearch />
			<table class="tree">
				<tbody>
					{propertyTree.map((x) =>
						<IfNotHidden key={x.key} path={x.path} >
							<PropertyTree node={x} />
						</IfNotHidden>
					)}
				</tbody>
			</table>
		</div>
	)
}


export function PropertySearch() {
	const [internalSearch, setInternalSearch] = useState("");


	const onSearchInput = (value: string) => {
		setInternalSearch(value);
		let query = value.toLowerCase();
		if (query.startsWith("0x")) {
			query = query.substring(2);
		}
		propertySearchSignal.value = query
	};

	return (
		<Show when={advancedModeSignal}>
			<label>Search property: </label>
			<span class="input-addon">
				<input type="text" value={internalSearch} onInput={x => onSearchInput(x.currentTarget.value)}></input>
				<IconButton
					disabled={!internalSearch}
					onClick={() => onSearchInput("")}
					title="Clear search"
					icon="clear"
				/>
			</span>
		</Show>
	);
}