import { useEffect, useSyncExternalStore } from "preact/compat";
import { Switch, Route, Redirect, useLocation } from "wouter";
import { LoadProgress } from "../components/LoadProgress";
import { PropertyEditor } from "../pages/propertyEditor/PropertyEditor";
import MapperPage from "../pages/mappers/MapperPage";
import { Store } from "../utility/propertyStore";
import { Settings } from "../pages/settings/Settings";
import { LicensePage } from "../pages/LicensePage";
import { usePrevious } from "../hooks/usePrevious";

export function MainView() {
	const isConnected = useSyncExternalStore(Store.subscribeConnected, Store.isConnected);
	const [path, setLocation] = useLocation();
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);	
	const previousMapper = usePrevious(mapper);

	useEffect(() => {
		if (path === "~/") {
			setLocation(mapper ? "/properties" : "/mapper/", { replace: false});
		}
	}, [path, mapper, setLocation]);

	useEffect(() => {
		if (mapper && !previousMapper) {
			setLocation("/properties", { replace: false});
			
		}
	}, [mapper, previousMapper, setLocation]);

	if (!isConnected) {
		return (
			<main class="loading">
				<LoadProgress label="Waiting for WebSocket connection" />
			</main>
		);
	}
	return (
		<main>
			<Switch>
				<Route path="/properties">
					<PropertyEditor />
				</Route>
				<Route path="/settings">
					<Settings />
				</Route>
				<Route path="/mapper" nest>
					<MapperPage />
				</Route>
				<Route path="/license/" nest>
					<LicensePage />
				</Route>
				<Route>
					<Redirect to={mapper ? "/properties": "/mapper/"} />
				</Route>
			</Switch>
		</main>
	);
}
