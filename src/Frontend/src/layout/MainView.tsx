
import { navigateTo, Switch } from "@/components/Route";
import { LoadProgress } from "../components/LoadProgress";
import { PropertyEditor } from "../pages/propertyEditor/PropertyEditor";
import MapperPage from "../pages/mappers/MapperPage";
import { Settings } from "../pages/settings/Settings";
import { LicensePage } from "../pages/license/LicensePage";
import { DepreciationNotices } from "./DepreciationNotices";
import { isConnectedSignal, mapperSignal } from "@/Contexts/mapperSignal";
import { useEffect } from "preact/hooks";

export function MainView() {
	const isConnected = isConnectedSignal.value;
	const mapper = mapperSignal.value;

	if (!isConnected) {
		return (
			<main class="loading">
				<LoadProgress label="Waiting for WebSocket connection" />
			</main>
		);
	}
	
	return (
		<main>
			<Switch map={[
				["/properties", PropertyEditor],
				["/settings", Settings],
				["/mappers", MapperPage],
				["/license", LicensePage],
				["*", DefaultRedirect],
			]} />
			<DepreciationNotices mapperId={mapper?.id ?? null}/>
		</main>
	);
}

function DefaultRedirect() {
	useEffect(() => {
		navigateTo(mapperSignal.value ? "/properties": "/mappers");
	});
	return null;
}
