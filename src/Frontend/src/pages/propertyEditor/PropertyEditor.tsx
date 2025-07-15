import { Store } from "../../utility/propertyStore";
import { PropertyTree } from "./components/PropertyTree";
import { unique } from "./utils/unique";
import { useEffect } from "preact/hooks";
import { useSyncExternalStore } from "preact/compat";
import { useLocation } from "wouter";
import { HidePropertyContextProvider, IfNotHidden, ToggleForceVisible } from "../../Contexts/HidePropertyContext";
import { Advanced } from "../../Contexts/Advanced";

export function PropertyEditor() {
	const [, setLocation] = useLocation();
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
		<HidePropertyContextProvider mapperId={mapper.id}>
			<div className="layout-box margin-top">
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
				<table className="tree">
					<tbody>
						{paths.map((x) => {
							return (
								<IfNotHidden key={x} path={x} >
									<PropertyTree path={x} />
								</IfNotHidden>
							);
						})}
					</tbody>
				</table>
			</div>
		</HidePropertyContextProvider>
	)
}