import { Store } from "../../utility/propertyStore";
import { PropertyTree } from "./components/PropertyTree";
import { unique } from "./utils/unique";
import { useEffect, useSyncExternalStore } from "react";
import { useLocation } from "wouter";


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
			setLocation("../mappers/");
		}
	}, [mapper, isConnected, setLocation])

	if (!isConnected) {
		return (
			<div id="property-editor">
				<h1>
					No connection to PokeAByte
				</h1>
			</div>
		);
	}

	return (
		<div className="layout-box margin-top">
			<span className="row">
				<strong className="small margin-right">
					Properties for {mapper?.gameName}
				</strong>
				<button type="button" className="border-red" onClick={Store.client.unloadMapper}>
					UNLOAD MAPPER
				</button>
			</span>
			{mapper?.gameName.toLowerCase().includes("deprecated") &&
				<p className="text-red">
					<small>
					This mapper is deprecated! As such, it will not be updated with new features.
					It will not have the same level of features or support as the latest mappers.
					<br />
					This one is provided so that users can continue to use software that was programmed using these property paths.
					</small>
				</p>
			}
			<ul className="tree">
				{paths.map((x) => {
					return <PropertyTree key={x} path={x} />
				})}
			</ul>
		</div>
	)
}