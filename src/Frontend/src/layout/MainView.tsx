import { useSyncExternalStore } from "react";
import { Switch, Route, Redirect } from "wouter";
import { LoadProgress } from "../components/LoadProgress";
import { PropertyEditor } from "../pages/propertyEditor/PropertyEditor";
import MapperPage from "../pages/mappers/MapperPage";
import { Store } from "../utility/propertyStore";
import { Settings } from "../pages/settings/Settings";
import { LicensePage } from "../pages/LicensePage";

export function MainView() {
	const isConnected = useSyncExternalStore(Store.subscribeConnected, Store.isConnected);
	if (!isConnected) {
		return (
			<main className="loading">
				<LoadProgress label="Waiting for WebSocket connection" />
			</main>
		);
	}
	return (
		<main className="responsive max surface-container-high">
			<Switch>
				<Route path="/properties">
					<PropertyEditor />
				</Route>
				<Route path="/settings">
					<Settings />
				</Route>
				<Route path={"/mapper"} nest>
					<MapperPage />
				</Route>
				<Route path={"/license/"} nest>
					<LicensePage />
				</Route>
				<Route>
					<Redirect to="/mapper/" />
				</Route>
			</Switch>
		</main>
	);
}
