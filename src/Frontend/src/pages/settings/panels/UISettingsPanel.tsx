import { useContext } from "preact/hooks";
import { Panel } from "../../../components/Panel";
import { UISettingsContext, useUISetting } from "@/Contexts/UISettingsContext";
import { FavoriteManagement } from "./components/FavoriteManagement";

export function UISettingsPanel() {
	const settingsContext = useContext(UISettingsContext);
	const [advancedMode, setAdanvedMode] = useUISetting("advancedMode");
	const [preserveFreeze, setPreserveFreeze] = useUISetting("preserveFreeze");
	const [forceVisible, setForceVisible] = useUISetting("forceVisible");
	const [recentlyUsedEnabled, setRecentlyUsed] = useUISetting("recentlyUsedEnabled");

	return (
		<Panel id="settings_ui" title="UI settings">
			<strong>
				Settings will apply as soon as you change them.
			</strong>
			<hr />
			<form onSubmit={(e) => e.preventDefault()}>
				<table class="striped">
					<tbody>
						<tr>
							<th>
								<label htmlFor="advanced">Enable advanced mode: </label>
							</th>
							<td>
								<input
									name="advanced"
									type="checkbox"
									role="switch"
									checked={advancedMode}
									onInput={() => setAdanvedMode(!advancedMode)}
								/>
								<span>Displays additional information and enables certain features.</span>
							</td>
						</tr>
						<tr>
							<th>
								<label htmlFor="forceVisible">Display hidden properties: </label>
							</th>
							<td>
								<input
									name="forceVisible"
									type="checkbox"
									role="switch"
									checked={forceVisible}
									onInput={() => setForceVisible(!forceVisible)}
								/>
								<span>Enabling this shows properties even if you chose to hide them. </span>
							</td>
						</tr>
						<tr>
							<th>
								<label htmlFor="preserveFreeze">Preserve freezes on reload: </label>
							</th>
							<td>
								<input
									name="preserveFreeze"
									type="checkbox"
									role="switch"
									checked={preserveFreeze}
									onInput={() => setPreserveFreeze(!preserveFreeze)}
								/>
								<span>When reloading a mapper, reapply previously frozen values. </span>
							</td>
						</tr>
						<tr>
							<th>
								<label htmlFor="recentlyUsed">Track recently used mappers:</label>
							</th>
							<td>
								<input
									name="recentlyUsed"
									type="checkbox"
									role="switch"
									checked={settingsContext.settings.recentlyUsedEnabled}
									onInput={() => setRecentlyUsed(!recentlyUsedEnabled)}
								/>
								<span> When enabled, keep track of the last 5 used mappers for quick loading </span>
							</td>
						</tr>
						<FavoriteManagement />
					</tbody>
				</table>
			</form>
		</Panel>
	)
}
