import { Store } from "../../utility/propertyStore";
import { PropertyTree } from "./components/PropertyTree";
import { unique } from "./utils/unique";
import { useEffect, useState } from "preact/hooks";
import { useSyncExternalStore } from "preact/compat";
import { useLocation } from "wouter";
import { HidePropertyContextProvider, IfNotHidden, ToggleForceVisible } from "../../Contexts/HidePropertyContext";
import { Advanced } from "../../Contexts/Advanced";

export function PropertyEditor() {
	const [, setLocation] = useLocation();
	const [search, setSearch] = useState("");
	const properties = Store.getAllProperties();
	const paths = Object.keys(properties)
		.map(x => x.split(".")[0])
		.filter(unique);
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);
	const isConnected = useSyncExternalStore(Store.subscribeConnected, Store.isConnected);
	useEffect(() => {
		if (!mapper && isConnected) {
			setLocation("/mappers/");
		}
	}, [mapper, isConnected, setLocation])

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
		<HidePropertyContextProvider mapperId={mapper.id} key="unique">
			<div className="layout-box margin-top" >
				<div class="title">
					<div>
						<strong>{mapper.gameName}</strong>
					</div>
					<div>
						<Advanced>
							<ToggleForceVisible/>
						</Advanced>
					</div>
				</div>
				<Advanced>
					<label>Search property: </label>
					<span class="input-addon">
						<input type="text" value={search} onChange={x => setSearch(x.currentTarget.value)}></input>
						<button 
							class={"add-on material-icons"} 
							disabled={!search}
							onClick={() => setSearch("")}
							title={"Clear search"}
						>
							clear
						</button>
					</span>
				</Advanced>
				<table className="tree">
					<tbody>
						{paths.map((x) => 
							<IfNotHidden key={x} path={x} >
								<PropertyTree path={x} search={search} />
							</IfNotHidden>
						)}
					</tbody>
				</table>
			</div>
		</HidePropertyContextProvider>
	)
}